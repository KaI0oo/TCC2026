import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.tree import DecisionTreeClassifier
from sklearn.calibration import CalibratedClassifierCV
import pickle

def gerar_modelo():
    df = pd.read_csv("./INTERFACE_POSTRATA/IA/dados_psa_clinica.csv")
    X = df.drop('Resultado', axis=1)
    y = df['Resultado']

    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.3, random_state=42)

    # Treinar modelo original
    clf = DecisionTreeClassifier(max_depth=3, criterion='entropy', random_state=42)
    clf.fit(X_train, y_train)

    # CALIBRAR o modelo (NOVO!)
    clf_calibrated = CalibratedClassifierCV(clf, cv=5, method='sigmoid')
    clf_calibrated.fit(X_train, y_train)

    with open("./INTERFACE_POSTRATA/IA/IA.pkl", "wb") as arquivo:
        pickle.dump(clf_calibrated, arquivo)