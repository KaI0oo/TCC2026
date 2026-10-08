    import sys
import os
import json
import pandas as pd
import joblib

def main():
    try:
        # 1. Locate Model File relative to the executable
        # If running as a script, use the script directory.
        # If running as a PyInstaller exe, sys.executable is the path to IA.exe
        if getattr(sys, 'frozen', False):
            # Running as bundled executable
            base_path = os.path.dirname(sys.executable)
        else:
            # Running as python script
            base_path = os.path.dirname(os.path.abspath(__file__))

        model_path = os.path.join(base_path, "IA.pkl")

        if not os.path.exists(model_path):
            raise FileNotFoundError(f"Model file IA.pkl not found at {model_path}")

        # Load the pre-trained model
        model = joblib.load(model_path)

        # 2. Parse Arguments
        # Expected: <PSA_Total> <PSA_Livre> <Densidade_PSA> <Idade>
        if len(sys.argv) < 5:
            raise ValueError("Insufficient arguments. Expected: <PSA_Total> <PSA_Livre> <Densidade_PSA> <Idade>")

        try:
            psa_total = float(sys.argv[1])
            psa_livre = float(sys.argv[2])
            densidade = float(sys.argv[3])
            idade = float(sys.argv[4])
        except ValueError:
            raise ValueError("Invalid numeric arguments provided.")

        # 3. Classification Logic (Preserved exactly)
        relacao_lt = psa_livre / psa_total if psa_total != 0 else 0
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

        resultado_val = model.predict(entrada)[0]
        resultado_txt = "SUSPEITO" if resultado_val == 1 else "BENIGNO"

        # 4. JSON Output
        print(json.dumps({"status": "success", "result": resultado_txt}))

    except Exception as e:
        # Ensure all errors are returned as JSON to stdout for C# to parse
        print(json.dumps({"status": "error", "message": str(e)}))
        sys.exit(1)

if __name__ == "__main__":
    main()
