FROM python:3.10-slim
WORKDIR /app
COPY ./requirements.txt /app
RUN pip install -r requirements.txt \
    && python -m spacy download uk_core_news_sm
COPY . /app
ENTRYPOINT [ "python", "app.py" ]