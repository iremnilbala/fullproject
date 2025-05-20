from flask import Flask, request, jsonify
from sentence_transformers import SentenceTransformer, util
import pyodbc

app = Flask(__name__)
model = SentenceTransformer('paraphrase-MiniLM-L6-v2')

# SQL Server bağlantısı
conn = pyodbc.connect(
    'Driver={SQL Server};'
    'Server=.\\SQLEXPRESS;'
    'Database=DefaultConnection;'
    'Trusted_Connection=yes;'
)

cursor = conn.cursor()
cursor.execute("SELECT Question, Answer FROM Faqs WHERE isDeleted = 0")
rows = cursor.fetchall()

# Soruları encode ederek önceden sakla
faq_data = [{
    "question": row[0],
    "answer": row[1],
    "embedding": model.encode(row[0], convert_to_tensor=True)
} for row in rows]

# Eşik değeri (benzerlik skoru)
SIMILARITY_THRESHOLD = 0.60  # daha esnek eşleşme için düşürüldü

@app.route("/get-answer", methods=["POST"])
def get_answer():
    user_question = request.json.get("question")

    # Çok kısa veya boş soruları engelle
    if not user_question or len(user_question.strip()) < 3:
        return jsonify({"answer": "Please enter a more complete question."})

    # Kullanıcı sorusunu embed et
    question_embedding = model.encode(user_question, convert_to_tensor=True)

    best_score = -1
    best_answer = None

    for faq in faq_data:
        score = util.pytorch_cos_sim(question_embedding, faq["embedding"]).item()
        print(f"Compared with: {faq['question']} | Score: {score:.4f}")  # Skoru logla
        if score > best_score:
            best_score = score
            best_answer = faq["answer"]

    # Eşik değeri kontrolü
    if best_score >= SIMILARITY_THRESHOLD:
        return jsonify({"answer": best_answer})
    else:
        return jsonify({"answer": "Sorry, I couldn't find a relevant answer in the database."})

if __name__ == "__main__":
    app.run(debug=True)
