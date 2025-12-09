import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestClassifier
import joblib
import os

CSV_PATH = r"D:\AIData\battle_data.csv"   # Unity가 만든 CSV
MODEL_PATH = r"D:\AIData\model.pkl"       # 학습된 모델 저장 경로

def main():
    if not os.path.exists(CSV_PATH):
        raise FileNotFoundError(f"CSV 파일이 없습니다: {CSV_PATH}")

    # 1) CSV 불러오기
    df = pd.read_csv(CSV_PATH, header=0)
    df.columns = [
        "playerHP", "enemyHP", "playerLastAction", "enemyLastAction", "turnNumber", "playerStreak", "enemyStreak", "playerDeltaHP", "enemyDeltaHP", "playerNextAction"
    ]

    # 2) Feature / Label 분리
    feature_cols = ["playerHP", "enemyHP", "playerLastAction", "enemyLastAction", "turnNumber", "playerStreak", "enemyStreak", "playerDeltaHP", "enemyDeltaHP",]
    X = df[feature_cols]
    y = df["playerNextAction"]

    # 3) Train/Test 분리
    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=0.2, random_state=42, stratify=y
    )

    # 4) 모델 선언
    model = RandomForestClassifier(
        n_estimators=300,
        random_state=42,
        n_jobs=-1
    )

    # 5) 학습
    model.fit(X_train, y_train)

    # 6) 정확도 출력
    print("Train Accuracy:", model.score(X_train, y_train))
    print("Test Accuracy :", model.score(X_test, y_test))

    # 7) 모델 저장
    os.makedirs(os.path.dirname(MODEL_PATH), exist_ok=True)
    joblib.dump(model, MODEL_PATH)
    print("모델 저장 완료:", MODEL_PATH)

if __name__ == "__main__":
    main()
