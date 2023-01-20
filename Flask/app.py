import os
import json
import psycopg2
import pandas as pd
from flask import Flask, jsonify, request
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.metrics.pairwise import cosine_similarity
from spacy.lang.uk.stop_words import STOP_WORDS as uk_stop

app = Flask(__name__)

@app.route("/content_based/<int:book_id>", methods=["GET"])
def hello(book_id = None):
    response = { "success" : True, "data" : [] }
    amount = request.args.get("amount", default=10, type=int)
    recommendations = get_recommendations(book_id, amount)

    return jsonify(recommendations)



def get_recommendations(book_id: int, amount: int = 10):
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