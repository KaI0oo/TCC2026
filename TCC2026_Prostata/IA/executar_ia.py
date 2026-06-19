import pickle
import pandas as pd
import sys
import os
from IA_generator import gerar_modelo

if not os.path.exists("./TCC2026_Prostata/IA/IA.pkl"):
    gerar_modelo()

pasta_atual = os.path.dirname(os.path.abspath(__file__))

caminho_modelo = os.path.join(pasta_atual, "IA.pkl")

with open(caminho_modelo, "rb") as arquivo:
    modelo = pickle.load(arquivo)

idade = float(sys.argv[1])
psa_total = float(sys.argv[2])
psa_livre = float(sys.argv[3])
densidade = float(sys.argv[4])

relacao_lt = psa_livre / psa_total

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

if resultado == 1:
    print("SUSPEITO")
else:
    print("BENIGNO")