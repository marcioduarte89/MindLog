from flask import Flask, request, jsonify
from sentence_transformers import SentenceTransformer
import numpy as np

app = Flask(__name__)

# Load model once at startup
model = SentenceTransformer('all-MiniLM-L6-v2')

@app.route('/sentences-transform', methods=['POST'])
def transform_sentences():
    try:
        data = request.get_json()
        
        if not isinstance(data, list):
            return jsonify({"error": "Input must be a list"}), 400

        response = []
        for item in data:
            if not isinstance(item, dict) or 'id' not in item or 'text' not in item:
                return jsonify({"error": "Each item must be a dict with 'id' and 'text'"}), 400

            sentence_id = item['id']
            text = item['text']
            embedding = model.encode(text).tolist()  # Convert numpy array to list

            response.append({
                "id": sentence_id,
                "embeddings": embedding
            })

        return jsonify(response), 200

    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    app.run(debug=True)
