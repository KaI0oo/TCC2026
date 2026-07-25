import os
import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.tree import DecisionTreeClassifier
from sklearn.calibration import CalibratedClassifierCV
from sklearn.metrics import classification_report, confusion_matrix
import pickle

pasta_atual = os.path.dirname(os.path.abspath(__file__))
caminho_csv = os.path.join(pasta_atual, "dados_psa_clinica.csv")

df = pd.read_csv(caminho_csv)
X = df.drop('Resultado', axis=1)
y = df['Resultado']

X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.3, random_state=42)
# Treinar modelo original
clf = DecisionTreeClassifier(max_depth=3, criterion='entropy', random_state=42)
clf.fit(X_train, y_train)
# CALIBRAR o modelo (NOVO!)
clf_calibrated = CalibratedClassifierCV(clf, cv=5, method='sigmoid')
clf_calibrated.fit(X_train, y_train)

def gerar_modelo():
    caminho_modelo = os.path.join(pasta_atual, "IA.pkl")

    with open(caminho_modelo, "wb") as arquivo:
        pickle.dump(clf_calibrated, arquivo)

def gerar_metricas_avaliacao():
    print(f"Acurácia do modelo calibrado: {(clf_calibrated.score(X_test, y_test) * 100):.2f}%")
    y_pred = clf_calibrated.predict(X_test)
    print("Relatório de Classificação:\n", classification_report(y_test, y_pred))
    tn, fp, fn, tp = confusion_matrix(y_test, y_pred).ravel()
    print("--- Valores Isolados da Matriz ---")
    print(f"Verdadeiros Negativos (Benignos Corretos): {tn}")
    print(f"Falsos Positivos (Alarmes Falsos): {fp}")
    print(f"Falsos Negativos (Casos perdidos!): {fn}")
    print(f"Verdadeiros Positivos (Suspeitos Corretos): {tp}")
    # print("Matriz de Confusão:\n", confusion_matrix(y_test, y_pred))
# gerar_modelo()
# gerar_metricas_avaliacao()