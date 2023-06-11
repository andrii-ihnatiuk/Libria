import os
import re
import json
import enum
import spacy
import atexit
import psycopg2
import numpy as np
import pandas as pd
from flask import Flask, jsonify, request
from sklearn.metrics.pairwise import cosine_similarity
from sklearn.preprocessing import MinMaxScaler, MultiLabelBinarizer
from spacy.lang.uk.stop_words import STOP_WORDS as uk_stop
from spacy.lang.en.stop_words import STOP_WORDS as en_stop
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.feature_extraction.text import TfidfVectorizer
from apscheduler.schedulers.background import BackgroundScheduler


class VectorizerType(enum.Enum):
    TFIDF = 0,
    COUNT = 1

DATA_DIR = "./data"
DF_PATH = DATA_DIR + "/df.csv"
SCORES_DIR = DATA_DIR + "/scores"

df = categ_cosine_sim = authr_cosine_sim = descr_cosine_sim = None

app = Flask(__name__)

@app.route("/content_based/<int:book_id>", methods=["GET"])
def recommend(book_id: int):
    response = { "success" : True, "data" : [] }
    amount = request.args.get("amount", default=10, type=int)
    recommendations = get_content_based(book_id, amount)

    if recommendations is None:
        response["success"] = False
    else:
        response["data"] = recommendations

    return jsonify(response)


def get_content_based(book_id: int, amount: int = 10) -> list[int] | None:
    """
    Find top N recommendations for given book

    Args:
        book_id (int): id of book to recommend to
        amount (int): number of recommendations returned
    Returns:
        list[int] of book ids if success or None if failed
    """
    if (df is None or categ_cosine_sim is None or authr_cosine_sim is None or descr_cosine_sim is None):
        return None

    if (book_id not in df["BookId"].values):
        return None

    # ваги для атрибутів
    descr_w = 5 # description weight
    categ_w = 1 # category weight
    authr_w = 1 # author weight
    # зважена сума подібностей за усіма властивостями (wighted sum)
    cosine_sim = categ_cosine_sim * categ_w + authr_cosine_sim * authr_w + descr_cosine_sim * descr_w

    # одержуємо індекс книги у датафреймі з ідентифікатора книги
    idx = df[df['BookId']==book_id].index.values[0]

    # одержуємо попарні значення схожості усіх книг з даною книгою
    sim_scores = cosine_sim[idx].reshape(-1, 1)

    # створюємо об'єкт нормалізатора та вказуємо максимальне та мінімальне значення для нормалізації
    scaler = MinMaxScaler(copy=False)
    scaler.fit([[descr_w + categ_w + authr_w], [0]])
    # нормалізуємо оцінки подібності у діапазон [0, 1]
    scaler.transform(sim_scores)

    # переходимо назад до 1-D масиву і зберігаємо оригінальні індекси
    sim_scores = list(enumerate(sim_scores.reshape(-1)))
    
    # фільтруємо за пороговим значенням схожості
    threshold = 0.135 # можна підняти коли книг буде значно більше, мале в демонстраційних цілях
    sim_scores = filter(lambda i: i[1] >= threshold, sim_scores)
    
    # сортуємо книги за їх схожістю з даною книгою
    sim_scores = sorted(sim_scores, key=lambda x: x[1], reverse=True)
    # одержуємо частину масиву з 10 найбільш схожими книгами
    # пропускаємо першу книгу, так як найбільш схожа - це вона сама
    sim_scores = sim_scores[1:amount+1] 

    # одержуємо індекси найбільш схожих книг
    recommend_ids = [i[0] for i in sim_scores]

    # app.logger.info(list(zip(list(df["BookId"].iloc[recommend_ids]), [item[-1] for item in sim_scores])))

    # повертаємо результат функції
    return list(df["BookId"].iloc[recommend_ids])


def score_text_similarity(df_column, vectorizer_type: VectorizerType, token_pattern=r"\b\w[\w’‘']+\b") -> np.ndarray:
    """
    Calculate cosine similarity between all books based on textual features

    Args:
        df_column (pandas Series): dataframe column to use
        vectorizer_type (VectorizerType): a method used to vectorize text data
        token_pattern (regex): regular expression used to tokenize text
    Returns:
        similarity score matrix with dimensions N * N (N - number of books)
    """
    stop = list(uk_stop)+list(en_stop)

    if vectorizer_type == VectorizerType.COUNT:
        vectorizer = CountVectorizer(stop_words=stop, token_pattern=token_pattern, min_df=2) 
    else:
        vectorizer = TfidfVectorizer(stop_words=stop, token_pattern=token_pattern, min_df=2) 
    
    count_matrix = vectorizer.fit_transform(df_column) # matrix with shape (Number of books, Number of unique words) 
    cosine_sim = cosine_similarity(count_matrix, count_matrix) # matrix with shape (Number of books, Number of books)
    app.logger.info("Shape of a matrix: {}".format(np.shape(count_matrix)))

    return cosine_sim


def load_memory_data():
    """
    Load saved dataframe and similarity matrices for each property from system memory
    """
    global df

    if (os.path.isfile(DF_PATH)):
        df = pd.read_csv(DF_PATH)
    else:
        recalculate_similarities() # if failed to load dataframe from memory fetch data from db
        return

    for name in ["categ_cosine_sim", "authr_cosine_sim", "descr_cosine_sim"]:
        file_path = SCORES_DIR + "/" + name
        try:
            globals()[name] = np.load(file_path)
        except OSError:
            app.logger.warning("Cannot load memory data for file '{0}'. Will recalculate all data from scratch.".format(file_path))
            recalculate_similarities() # if at least one file failed to load then fetch all data from db
            break


def recalculate_similarities():
    """
    Load data from Database, clean, calculate similarities and save data to memory
    """
    app.logger.info("Recalculating similarities")
    global df, categ_cosine_sim, authr_cosine_sim, descr_cosine_sim
    data = fetch_books()
    if (data is None):
        app.logger.warning("No data received to recalculate similarity scores. The function will be aborted.")
        return
    df = pd.DataFrame(data, columns=["BookId", "Title", "Description", "Authors", "Categories"])

    # --- Clean data and perform heavy calculations

    df.fillna("", inplace=True)
    df['Description'] = df['Description'].str.cat(df['Title'], sep=' ') # поєднуємо заголовки з описом

    # перетворення тексту у нижній регістр та стандартизація апострофів
    df["Description"] = df["Description"].apply(lambda x: re.sub(r"[‘’′´`]", "'", x).lower())

    # розбиття строк на масив міток "Label 1, Label 2" => ["Label 1", "Label 2"]
    df["Authors"] = df['Authors'].str.split(", ") # розбиваємо рядок з id авторів на масив міток
    df["Categories"] = df['Categories'].str.split(", ") # розбиваємо рядок з id категорій на масив міток
    
    mlb = MultiLabelBinarizer() # бінаризатор ознак з довільною кількістю міток
    # кодування ознак з довільною кількістю міток (книга може мати від 1 до N авторів та категорій)
    authors_binarized = mlb.fit_transform(df["Authors"])
    categories_binarized = mlb.fit_transform(df["Categories"])

    # завантаження навченої мовної моделі Spacy (українська мова)
    nlp = spacy.load("uk_core_news_sm")
    # додаємо до стандартного списку стоп-слів моделі англійські стоп-слова
    nlp.Defaults.stop_words |= en_stop
    # лематизація слів - приведення до початкової форми за допомогою нейромережі
    # перед викликом lemma_ перевіряємо токен на стоп-слово щоб дарма не витрачати час
    df["Description"] = df["Description"].apply(lambda x: " ".join([token.lemma_ for token in nlp(x) if not token.is_stop]))

    # розраховуємо взаємну схожість кожної книги за кожним критерієм 
    categ_cosine_sim = cosine_similarity(categories_binarized)
    authr_cosine_sim = cosine_similarity(authors_binarized)
    descr_cosine_sim = score_text_similarity(df["Description"], VectorizerType.TFIDF)

    # видаляємо непотрібні стовпці, лише BookId потрібне для видачі рекомендацій пізніше
    df.drop(["Title", "Description", "Authors", "Categories"], axis=1, inplace=True)

    # save dataframe
    if (os.path.exists(DATA_DIR) == False):
        os.mkdir(DATA_DIR)
    df.to_csv(DF_PATH)

    # save calculated similarity scores for all properties
    if (os.path.exists(SCORES_DIR) == False):
        os.mkdir(SCORES_DIR)
    for name in ["categ_cosine_sim", "authr_cosine_sim", "descr_cosine_sim"]:
        file_path = SCORES_DIR + "/" + name
        with open(file_path, "wb+") as f:
            np.save(f, globals()[name])

    app.logger.info("Successfully processed data")


def fetch_books() -> list[tuple] | None:
    """
    Request all books from Database
    """
    options = app.config["POSTGRES_CONNECTION"]
    query = \
    """
    SELECT 
        b."BookId", 
        b."Title", 
        b."Description", 
        string_agg(distinct ab."AuthorsAuthorId"::character varying, ', '),
        string_agg(distinct bc."CategoriesCategoryId"::character varying, ', ')
    FROM 
        "Books" AS b 

    -- AUTHORS
    LEFT JOIN "AuthorBook" AS ab ON 
        ab."BooksBookId" = b."BookId" 

    -- CATEGORIES
    LEFT JOIN "BookCategory" AS bc ON 
        bc."BooksBookId" = b."BookId"

    GROUP BY b."BookId"
    """
    records = None
    try:
        conn = psycopg2.connect(
            dbname=options["database"], 
            user=options["user_id"], 
            password=options["password"], 
            host=options["server"],
            port=options["port"]
        )
    except psycopg2.OperationalError as err:
        app.logger.error("Error while connecting to DB: {0}".format(err))
        conn = None

    if (conn is not None):
        cursor = conn.cursor()
        try:
            cursor.execute(query)
            records = cursor.fetchall()
        except Exception as err:
            app.logger.error("Error while executing query: {0}".format(err))
            conn.rollback()

        cursor.close()
        conn.close()

    return records


if __name__ == "__main__":
    config_location = os.environ.get("SECRETS")
    if not config_location:
        raise Exception("Environment variable for secrets location was not provided!")
    
    # schedule a background job to recalculate books' similarity scores
    scheduler = BackgroundScheduler()
    scheduler.add_job(func=recalculate_similarities, trigger="cron", hour=0) # run every day at 0 o'clock
    scheduler.start()
    # Shut down the scheduler when exiting the app
    atexit.register(lambda: scheduler.shutdown())

    config_location+="/flask_config.json"
    app.config.from_file(config_location, load=json.load)

    # if no data is stored or if file is empty load all data on app startup
    if (os.path.isfile(DF_PATH) == False or os.path.getsize(DF_PATH) == 0):
        recalculate_similarities()
    else: # if dataframe file found, try to load all data from memory
        load_memory_data()

    app.run(host="0.0.0.0", port=7007, debug=True, use_reloader=False)