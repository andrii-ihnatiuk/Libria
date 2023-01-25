import os
import json
import re
import enum
import psycopg2
import pandas as pd
from flask import Flask, jsonify, request
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity
from spacy.lang.uk.stop_words import STOP_WORDS as uk_stop
from spacy.lang.en.stop_words import STOP_WORDS as en_stop

class VectorizerType(enum.Enum):
    TFIDF = 0,
    COUNT = 1

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
    data = fetch_books()
    if data.count == 0:
        return None
    # створення датафрейму з отриманий даних
    df = pd.DataFrame(data, columns=["BookId", "Title", "Description", "Authors", "Categories"])
    df.fillna("", inplace=True)

    if (book_id not in df["BookId"].values):
        return None

    # перетворення тексту у нижній регістр
    for feature in ["Title", "Description", "Authors", "Categories"]:
        df[feature] = df[feature].apply(lambda x: x.lower())

    # видалення пробілів, наприклад щоб Іван (Франко) != Іван (Багряний).
    # алгоритм порахує що Іван з'явився 2 рази (окремо від прізвищ), хоча це для нас 
    # ніякої цінності не має. А от якщо з'явиться іванфранко, тоді ми можемо це використати
    df["Authors"] = df["Authors"].apply(lambda x: re.sub(r"[.\- ]", "", x))
    df["Categories"] = df["Categories"].apply(lambda x: re.sub(r"[.\- ]", "", x))

    title_cosine_sim = score_similarity(df["Title"], VectorizerType.COUNT)
    categ_cosine_sim = score_similarity(df["Categories"], VectorizerType.COUNT)
    auth_cosine_sim = score_similarity(df["Authors"], VectorizerType.COUNT)
    desc_cosine_sim = score_similarity(df["Description"], VectorizerType.TFIDF)

    # ваги для атрибутів
    desc_w = 4 # description weight
    title_w = 1 # title weigth
    categ_w = 1 # category weight
    auth_w = 1 # author weight
    cosine_sim = title_cosine_sim*title_w + categ_cosine_sim*categ_w + auth_cosine_sim * auth_w + desc_cosine_sim * desc_w

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
    sim_scores = sim_scores[1:amount + 1] 

    # одержуємо індекси найбільш схожих книг
    recommend_ids = [i[0] for i in sim_scores]

    # app.logger.info(list(zip(list(df["BookId"].iloc[recommend_ids]), [item[-1] for item in sim_scores])))

    # повертаємо результат функції
    return list(df["BookId"].iloc[recommend_ids])


def score_similarity(df_column, vectorizer_type: VectorizerType, token_pattern=r"\b\w[\w’‘']+\b"):
    stop = list(uk_stop)+list(en_stop)

    if vectorizer_type == VectorizerType.COUNT:
        vectorizer = CountVectorizer(stop_words=stop, token_pattern=token_pattern) 
    else:
        vectorizer = TfidfVectorizer(stop_words=stop, token_pattern=token_pattern) 

    count_matrix = vectorizer.fit_transform(df_column)
    cosine_sim = cosine_similarity(count_matrix, count_matrix)

    return cosine_sim


def fetch_books() -> list[tuple]:
    options = app.config["POSTGRES_CONNECTION"]
    conn = psycopg2.connect(dbname=options["database"], user=options["user_id"], 
                            password=options["password"], host=options["server"], port=options["port"])
    cursor = conn.cursor()
    cursor.execute(
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
    )
    records = cursor.fetchall()
    cursor.close()
    conn.close()

    return records


if __name__ == "__main__":
    config_location = os.environ.get("SECRETS")
    if not config_location:
        raise Exception("Environment variable for secrets location was not provided!")
    
    config_location+="/flask_config.json"
    app.config.from_file(config_location, load=json.load)

    app.run(host="0.0.0.0", port=7007, debug=True)