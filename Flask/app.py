from flask import Flask

app = Flask(__name__)

@app.route('/')
def hello():
    return 'This Compose/Flask demo has been viewed 1 time.'


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=7007, debug=True)