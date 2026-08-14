import pandas as pd
from pathlib import Path
import joblib
from sklearn.model_selection import train_test_split, GridSearchCV
from sklearn.tree import DecisionTreeClassifier
from sklearn.ensemble import RandomForestClassifier
from sklearn.calibration import CalibratedClassifierCV
from sklearn.metrics import classification_report, confusion_matrix, recall_score, accuracy_score

def carregar_dados(caminho_arquivo):
    """
    Carrega o dataset e separa as features da variável alvo (target).
    
    Parâmetros:
        caminho_arquivo (str ou Path): Caminho absoluto ou relativo para o CSV.
        
    Retorna:
        X (DataFrame): Variáveis independentes (features).
        y (Series): Variável dependente (target).
    """
    df = pd.read_csv(caminho_arquivo)
    X = df.drop('Resultado', axis=1)
    y = df['Resultado']
    return X, y

def treinar_modelo_prostata(X, y, test_size=0.3, random_state=42):
    """
    Realiza o particionamento estratificado dos dados, compara os algoritmos 
    Arvore de Decisao e Random Forest otimizando hiperparametros via GridSearchCV 
    com foco na metrica de Recall, e aplica a calibracao de probabilidades no 
    modelo vencedor.
    
    Parâmetros:
        X (DataFrame): Features de treinamento.
        y (Series): Target de treinamento.
        test_size (float): Proporção dos dados dedicada para teste.
        random_state (int): Semente para reprodutibilidade.
        
    Retorna:
        modelo_calibrado: O melhor modelo otimizado e calibrado.
        X_test (DataFrame): Features do conjunto de teste.
        y_test (Series): Target do conjunto de teste.
    """
    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=test_size, random_state=random_state, stratify=y
    )

    modelos_para_testar = {
        'Arvore_Decisao': {
            'modelo': DecisionTreeClassifier(class_weight='balanced', random_state=random_state),
            'parametros': {
                'max_depth': [3, 5, 7, None],
                'criterion': ['gini', 'entropy']
            }
        },
        'Random_Forest': {
            'modelo': RandomForestClassifier(class_weight='balanced', random_state=random_state),
            'parametros': {
                'max_depth': [3, 5, 7, None],
                'n_estimators': [50, 100, 200]
            }
        }
    }

    melhor_recall_cv = 0
    melhor_modelo_base = None

    for nome, config in modelos_para_testar.items():
        grid = GridSearchCV(
            estimator=config['modelo'],
            param_grid=config['parametros'],
            cv=5,
            scoring='recall',
            n_jobs=-1
        )
        
        grid.fit(X_train, y_train)
        
        if grid.best_score_ > melhor_recall_cv:
            melhor_recall_cv = grid.best_score_
            melhor_modelo_base = grid.best_estimator_

    clf_calibrated = CalibratedClassifierCV(melhor_modelo_base, cv=5, method='sigmoid')
    clf_calibrated.fit(X_train, y_train)

    return clf_calibrated, X_test, y_test

def gerar_metricas_avaliacao(modelo, X_test, y_test):
    """
    Avalia o modelo utilizando o conjunto de teste, focando no Recall 
    e na desconstrução da Matriz de Confusão.
    
    Parâmetros:
        modelo: Modelo de classificação treinado.
        X_test (DataFrame): Dados independentes de teste.
        y_test (Series): Target de teste.
        
    Retorna:
        dict: Dicionário contendo as métricas brutas (acurácia, recall, matriz) 
              e uma string multilinha (relatorio_texto) pronta para ser salva em logs 
              ou exibida pela aplicação chamadora.
    """
    y_pred = modelo.predict(X_test)
    
    acuracia = accuracy_score(y_test, y_pred)
    recall = recall_score(y_test, y_pred)
    tn, fp, fn, tp = confusion_matrix(y_test, y_pred).ravel()
    
    relatorio_sklearn = classification_report(y_test, y_pred)
    
    relatorio_texto = (
        "========================================\n"
        "METRICAS PRINCIPAIS NO TESTE (Foco Medico)\n"
        "========================================\n"
        f"Acuracia Geral: {acuracia * 100:.2f}%\n"
        f"Recall (Sensibilidade): {recall * 100:.2f}%\n\n"
        "--- Relatorio de Classificacao Completo ---\n"
        f"{relatorio_sklearn}\n"
        "--- Matriz de Confusao ---\n"
        f"VN (Saudaveis Corretos): {tn} | FP (Alarmes Falsos): {fp}\n"
        f"FN (Casos Perdidos): {fn}     | VP (Suspeitos Corretos): {tp}\n"
    )

    return {
        "acuracia": acuracia,
        "recall": recall,
        "matriz_confusao": {"vn": tn, "fp": fp, "fn": fn, "vp": tp},
        "relatorio_texto": relatorio_texto
    }

def salvar_modelo(modelo, caminho_salvamento):
    """
    Garante a criação dos diretórios necessários e salva o modelo no formato joblib.
    
    Parâmetros:
        modelo: Modelo treinado a ser salvo.
        caminho_salvamento (str ou Path): Caminho de destino do arquivo .joblib.
        
    Retorna:
        bool: True indicando que a gravação ocorreu com sucesso.
    """
    caminho = Path(caminho_salvamento)
    caminho.parent.mkdir(parents=True, exist_ok=True)
    joblib.dump(modelo, caminho)
    return True

def carregar_modelo_salvo(caminho_modelo):
    """
    Carrega o arquivo do modelo serializado e o retorna pronto para uso.
    
    Parâmetros:
        caminho_modelo (str ou Path): Caminho absoluto ou relativo para o arquivo salvo.
        
    Retorna:
        modelo: O objeto preditivo instanciado.
    """
    return joblib.load(caminho_modelo)