from flask import Flask, request, jsonify
import joblib
import numpy as np
import os

BASE = os.path.dirname(os.path.abspath(__file__))
MODEL_PATH = os.path.join(BASE, "..", "AIData", "model.pkl")

# 모델 로드
model = joblib.load(MODEL_PATH)

app = Flask(__name__)

@app.route("/predict", methods=["POST"])
def predict():
    data = request.get_json()

    # JSON에서 state 배열을 읽음
    # state = [playerHP, enemyHP, playerLastAction, enemyLastAction,
    #          turnNumber, playerStreak, enemyStreak, deltaPlayerHP, deltaEnemyHP]
    state = data.get("state")

    # numpy array 변환
    X = np.array(state, dtype=float).reshape(1, -1)

    # 예측 (가장 확률이 높은 행동)
    pred = model.predict(X)[0]

    # 예측 확률 전체
    proba = model.predict_proba(X)[0]

    return jsonify({
        "predictedAction": int(pred),
        "pAttack": float(proba[0]),
        "pCounter": float(proba[1]),
        "pHeal": float(proba[2])
    })


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000)
