import pandas as pd
from pathlib import Path
import joblib
from sklearn.model_selection import train_test_split
from sklearn.tree import DecisionTreeClassifier
from sklearn.calibration import CalibratedClassifierCV
from sklearn.metrics import classification_report, confusion_matrix, recall_score, accuracy_score

def carregar_dados(caminho_arquivo):
    """
    Carrega o dataset e separa as features do target.
    
    Parâmetros:
        caminho_arquivo (str ou Path): Caminho absoluto ou relativo para o CSV.
    Retorna:
        X (DataFrame), y (Series)
    """
    df = pd.read_csv(caminho_arquivo)
    X = df.drop('Resultado', axis=1)
    y = df['Resultado']
    return X, y

def treinar_modelo_prostata(X, y, test_size=0.3, random_state=42):
    """
    Realiza o split estratificado, treina a árvore de decisão balanceada 
    e aplica a calibração de probabilidades.
    
    Retorna:
        modelo_calibrado, X_test, y_test
    """
    # 1. Split com Estratificação (Garante proporção igual de doentes no treino e teste)
    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=test_size, random_state=random_state, stratify=y
    )

    # 2. Treinamento da Árvore de Decisão Base
    clf = DecisionTreeClassifier(
        max_depth=3, 
        criterion='entropy', 
        class_weight='balanced', # Penaliza erros na classe minoritária
        random_state=random_state
    )
    clf.fit(X_train, y_train)

    # 3. Calibração (Gera probabilidades confiáveis para o médico)
    clf_calibrated = CalibratedClassifierCV(clf, cv=5, method='sigmoid')
    clf_calibrated.fit(X_train, y_train)

    return clf_calibrated, X_test, y_test

def gerar_metricas_avaliacao(modelo, X_test, y_test, imprimir=True):
    """
    Avalia o modelo e retorna as métricas principais.
    
    Parâmetros:
        modelo: Modelo treinado pelo Scikit-Learn.
        X_test, y_test: Dados de teste.
        imprimir (bool): Se True, imprime o relatório no console.
        
    Retorna:
        dict: Dicionário contendo acurácia, recall e os valores da matriz de confusão.
    """
    y_pred = modelo.predict(X_test)
    
    acuracia = accuracy_score(y_test, y_pred)
    recall = recall_score(y_test, y_pred)
    tn, fp, fn, tp = confusion_matrix(y_test, y_pred).ravel()
    
    if imprimir:
        print("\n" + "="*40)
        print("MÉTRICAS PRINCIPAIS (Foco Médico)")
        print("="*40)
        print(f"Acurácia Geral: {acuracia * 100:.2f}%")
        print(f"Recall (Sensibilidade): {recall * 100:.2f}% (Capacidade de detectar doentes)")
        print("\n--- Relatório de Classificação Completo ---")
        print(classification_report(y_test, y_pred))
        print("--- Matriz de Confusão ---")
        print(f"VN (Saudáveis Corretos): {tn} | FP (Alarmes Falsos): {fp}")
        print(f"FN (Casos Perdidos): {fn}     | VP (Suspeitos Corretos): {tp}")

    # Retorna os dados para que o sistema chamador possa usá-los (ex: em uma API)
    return {
        "acuracia": acuracia,
        "recall": recall,
        "matriz_confusao": {"vn": tn, "fp": fp, "fn": fn, "vp": tp}
    }

def salvar_modelo(modelo, caminho_salvamento):
    """
    Salva o modelo treinado em disco usando joblib.
    """
    caminho = Path(caminho_salvamento)
    caminho.parent.mkdir(parents=True, exist_ok=True)
    joblib.dump(modelo, caminho)
    return True

def carregar_modelo_salvo(caminho_modelo):
    """
    Carrega um modelo previamente salvo do disco para realizar inferências.
    """
    return joblib.load(caminho_modelo)