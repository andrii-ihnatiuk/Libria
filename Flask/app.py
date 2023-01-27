import os
import re
import json
import enum
import atexit
import psycopg2
import numpy as np
import pandas as pd
from flask import Flask, jsonify, request
from sklearn.metrics.pairwise import cosine_similarity
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

df = title_cosine_sim = categ_cosine_sim = authr_cosine_sim = descr_cosine_sim = None

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



def get_content_based(book_id: int, amount: int = 10):
    if (df is None or title_cosine_sim is None or categ_cosine_sim is None or authr_cosine_sim is None or descr_cosine_sim is None):
        return None

    if (book_id not in df["BookId"].values):
        return None

    # ваги для атрибутів
    descr_w = 5 # description weight
    title_w = 1 # title weigth
    categ_w = 1 # category weight
    authr_w = 1 # author weight
    cosine_sim = title_cosine_sim * title_w + categ_cosine_sim * categ_w + authr_cosine_sim * authr_w + descr_cosine_sim * descr_w

    # одержуємо індекс книги у датафреймі з ідентифікатора книги
    idx = df[df['BookId']==book_id].index.values[0]
    # одержуємо попарні значення схожості усіх книг з даною книгою
    sim_scores = list(enumerate(cosine_sim[idx]))
    
    # фільтруємо за пороговим значенням схожості
    threshold = 1.1 # will be adjusted in future
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


def score_similarity(df_column, vectorizer_type: VectorizerType, token_pattern=r"\b\w[\w’‘']+\b") -> np.ndarray:
    stop = list(uk_stop)+list(en_stop)

    if vectorizer_type == VectorizerType.COUNT:
        vectorizer = CountVectorizer(stop_words=stop, token_pattern=token_pattern) 
    else:
        vectorizer = TfidfVectorizer(stop_words=stop, token_pattern=token_pattern) 

    count_matrix = vectorizer.fit_transform(df_column)
    cosine_sim = cosine_similarity(count_matrix, count_matrix)

    return cosine_sim


def load_memory_data():
    global df

    if (os.path.isfile(DF_PATH)):
        df = pd.read_csv(DF_PATH)
    else:
        recalculate_similarities() # if failed to load dataframe from memory fetch data from db
        return

    for name in ["title_cosine_sim", "categ_cosine_sim", "authr_cosine_sim", "descr_cosine_sim"]:
        file_path = SCORES_DIR + "/" + name
        try:
            globals()[name] = np.load(file_path)
        except OSError:
            app.logger.warning("Cannot load memory data for file '{0}'. Will recalculate all data from scratch.".format(file_path))
            recalculate_similarities() # if at least one file failed to load then fetch all data from db
            break


def recalculate_similarities():
    global df, title_cosine_sim, categ_cosine_sim, authr_cosine_sim, descr_cosine_sim
    data = fetch_books()
    if (data is None):
        app.logger.warning("No data received to recalculate similarity scores. The function will be aborted.")
        return
    df = pd.DataFrame(data, columns=["BookId", "Title", "Description", "Authors", "Categories"])

    # --- Clean data and perform heavy calculations

    df.fillna("", inplace=True)
    # перетворення тексту у нижній регістр
    for feature in ["Title", "Description", "Authors", "Categories"]:
        df[feature] = df[feature].apply(lambda x: x.lower())

    # видалення пробілів, наприклад щоб Іван (Франко) != Іван (Багряний).
    # алгоритм порахує що Іван з'явився 2 рази (окремо від прізвищ), хоча це для нас 
    # ніякої цінності не має. А от якщо з'явиться іванфранко, тоді ми можемо це використати
    df["Authors"] = df["Authors"].apply(lambda x: re.sub(r"[.\- ]", "", x))
    df["Categories"] = df["Categories"].apply(lambda x: re.sub(r"[.\- ]", "", x))

    # розраховуємо взаємну схожість кожної книги за кожним критерієм 
    title_cosine_sim = score_similarity(df["Title"], VectorizerType.COUNT)
    categ_cosine_sim = score_similarity(df["Categories"], VectorizerType.COUNT)
    authr_cosine_sim = score_similarity(df["Authors"], VectorizerType.COUNT)
    descr_cosine_sim = score_similarity(df["Description"], VectorizerType.TFIDF)

    # save dataframe
    if (os.path.exists(DATA_DIR) == False):
        os.mkdir(DATA_DIR)
    df.to_csv(DF_PATH)

    # save calculated similarity scores for all properties
    if (os.path.exists(SCORES_DIR) == False):
        os.mkdir(SCORES_DIR)
    for name in ["title_cosine_sim", "categ_cosine_sim", "authr_cosine_sim", "descr_cosine_sim"]:
        file_path = SCORES_DIR + "/" + name
        with open(file_path, "wb+") as f:
            np.save(f, globals()[name])


def fetch_books() -> list[tuple] | None:
    options = app.config["POSTGRES_CONNECTION"]
    query = \
    """
    SELECT 
        b."BookId", 
        b."Title", 
        b."Description", 
        string_agg(distinct a."Name", ', '),
        string_agg(distinct c."Name", ', ')
    FROM 
        "Books" AS b 

    -- AUTHORS
    LEFT JOIN "AuthorBook" AS ab ON 
        ab."BooksBookId" = b."BookId" 
    LEFT JOIN "Authors" AS a ON 
        a."AuthorId" = ab."AuthorsAuthorId" 

    -- CATEGORIES
    LEFT JOIN "BookCategory" AS bc ON 
        bc."BooksBookId" = b."BookId"
    LEFT JOIN "Categories" AS c ON
        c."CategoryId" = bc."CategoriesCategoryId"

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
    else:
        load_memory_data()

    app.run(host="0.0.0.0", port=7007, debug=True, use_reloader=False)