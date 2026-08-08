import sys
import pandas as pd
from pathlib import Path
import IA_generator as ia  # Importando a nova biblioteca criada

# 1. Gerenciamento de Caminhos Relativos Seguros
pasta_atual = Path(__file__).resolve().parent if '__file__' in globals() else Path.cwd()
caminho_modelo = pasta_atual / "IA.joblib" # Atualizado para .joblib
caminho_dados = pasta_atual / "dados_psa_clinica.csv" # Necessário caso precise treinar

# 2. Verifica se o modelo já existe. Se não existir, utiliza a biblioteca para gerar um.
if not caminho_modelo.exists():
    try:
        # Puxa o fluxo de treinamento da biblioteca
        X, y = ia.carregar_dados(caminho_dados)
        modelo_novo, _, _ = ia.treinar_modelo_prostata(X, y)
        ia.salvar_modelo(modelo_novo, caminho_modelo)
    except FileNotFoundError:
        print("ERRO: Modelo não encontrado e arquivo de dados (CSV) ausente para treino.")
        sys.exit(1)

# 3. Carrega o modelo utilizando a função da biblioteca
modelo = ia.carregar_modelo_salvo(caminho_modelo)

# 4. Processamento dos argumentos recebidos pelo sistema/backend
idade = float(sys.argv[1])
psa_total = float(sys.argv[2])
psa_livre = float(sys.argv[3])
densidade = float(sys.argv[4])

relacao_lt = psa_livre / psa_total
if relacao_lt > 1:
    relacao_lt /= 100
    
entrada = pd.DataFrame(
    [[psa_total, psa_livre, relacao_lt, idade, densidade]],
    columns=[
        "PSA_Total",
        "PSA_Livre",
        "PSA_Relacao_L/T",
        "Idade",
        "PSA_Densidade"
    ]
)

resultado = modelo.predict(entrada)[0]

# --- AS ÚLTIMAS LINHAS MANTIDAS INTACTAS PARA COMUNICAÇÃO COM O PROJETO ---
if resultado == 1:
    print("SUSPEITO")
      # Probabilidade de ser SUSPEITO
else:
    print("BENIGNO")
# gerar_metricas_avaliacao()