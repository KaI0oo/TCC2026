import pandas as pd
import mysql.connector
import re

from sklearn.model_selection import train_test_split
from sklearn.tree import DecisionTreeClassifier
from sklearn.calibration import CalibratedClassifierCV

# ============================================================
# VARIÁVEIS GLOBAIS
# ============================================================

host = "localhost"
user = "root"
password = ""
database = "banco_de_dados_prostata"

medico_logado = None

emails_validos = [

    "@gmail.com",
    "@hotmail.com",
    "@outlook.com",
    "@yahoo.com"

]

# ============================================================
# CONEXÃO MYSQL
# ============================================================

conexao = mysql.connector.connect(

    host=host,
    user=user,
    password=password,
    database=database

)

cursor = conexao.cursor()

print("\nBanco conectado com sucesso!\n")

# ============================================================
# IA
# ============================================================

df = pd.read_csv("dados_psa_clinica.csv")

X = df.drop('Resultado', axis=1)

y = df['Resultado']

X_train, X_test, y_train, y_test = train_test_split(

    X,
    y,
    test_size=0.3,
    random_state=42

)

clf = DecisionTreeClassifier(

    max_depth=3,
    criterion='entropy',
    random_state=42

)

clf.fit(X_train, y_train)

clf_calibrated = CalibratedClassifierCV(

    clf,
    cv=5,
    method='sigmoid'

)

clf_calibrated.fit(X_train, y_train)

# ============================================================
# VALIDAÇÕES
# ============================================================

def validar_cpf(cpf):

    cpf = ''.join(filter(str.isdigit, cpf))

    if len(cpf) != 11:

        return False

    return True

# ============================================================

def validar_telefone(telefone):

    telefone = ''.join(filter(str.isdigit, telefone))

    if len(telefone) < 10 or len(telefone) > 11:

        return False

    return True

# ============================================================

def validar_email(email):

    return any(email.endswith(x) for x in emails_validos)

# ============================================================

def formatar_nome(nome):

    return nome.strip().title()

# ============================================================

def formatar_doencas(doencas):

    lista = doencas.split(",")

    lista_formatada = []

    for doenca in lista:

        lista_formatada.append(doenca.strip().capitalize())

    return ", ".join(lista_formatada)

# ============================================================
# CADASTRAR MÉDICO
# ============================================================

def cadastrar_medico():

    global medico_logado

    print("\n========== CADASTRO MÉDICO ==========\n")

    while True:

        rm = input("RM: ").strip()

        if not rm.isdigit():

            print("RM inválido!")

        else:

            break

    sql = "SELECT * FROM medicos WHERE rm = %s"

    cursor.execute(sql, (rm,))

    resultado = cursor.fetchone()

    if resultado:

        print("\nRM já cadastrado!\n")
        return

    while True:

        nome = formatar_nome(input("Nome: "))

        if len(nome) < 3:

            print("Nome inválido!")

        else:

            break

    while True:

        especialidade = formatar_nome(input("Especialidade: "))

        if len(especialidade) < 3:

            print("Especialidade inválida!")

        else:

            break

    while True:

        telefone = input("Telefone: ")

        if not validar_telefone(telefone):

            print("Telefone inválido!")

        else:

            telefone = ''.join(filter(str.isdigit, telefone))
            break

    while True:

        email = input("Email: ").strip().lower()

        if not validar_email(email):

            print("Email inválido!")

        else:

            break

    while True:

        senha = input("Senha: ").strip()

        if len(senha) < 4:

            print("Senha muito curta!")

        else:

            break

    sql = """
    INSERT INTO medicos (

        rm,
        nome,
        especialidade,
        telefone,
        email,
        senha

    )

    VALUES (%s,%s,%s,%s,%s,%s)
    """

    valores = (

        rm,
        nome,
        especialidade,
        telefone,
        email,
        senha

    )

    cursor.execute(sql, valores)

    conexao.commit()

    print("\nMédico cadastrado com sucesso!\n")

# ============================================================
# LOGIN
# ============================================================

def login_medico():

    global medico_logado

    print("\n========== LOGIN ==========\n")

    rm = input("RM: ")

    senha = input("Senha: ")

    sql = """
    SELECT * FROM medicos
    WHERE rm = %s AND senha = %s
    """

    valores = (rm, senha)

    cursor.execute(sql, valores)

    resultado = cursor.fetchone()

    if resultado:

        medico_logado = rm

        print("\nLogin realizado!\n")

    else:

        print("\nRM ou senha inválidos!\n")

# ============================================================
# LOGOUT
# ============================================================

def logout():

    global medico_logado

    medico_logado = None

    print("\nLogout realizado!\n")

# ============================================================
# CADASTRAR PACIENTE
# ============================================================

def cadastrar_paciente():

    print("\n========== CADASTRO PACIENTE ==========\n")

    while True:

        cpf = input("CPF: ")

        if not validar_cpf(cpf):

            print("CPF inválido!")

        else:

            break

    sql = "SELECT * FROM pacientes WHERE cpf = %s"

    cursor.execute(sql, (cpf,))

    resultado = cursor.fetchone()

    if resultado:

        print("\nCPF já cadastrado!\n")
        return

    while True:

        nome = formatar_nome(input("Nome: "))

        if len(nome) < 3:

            print("Nome inválido!")

        else:

            break

    while True:

        idade = input("Idade: ")

        if not idade.isdigit():

            print("Idade inválida!")

        else:

            idade = int(idade)
            break

    while True:

        sexo = input("Sexo (M/F): ").upper()

        if sexo not in ["M", "F"]:

            print("Sexo inválido!")

        else:

            break

    while True:

        telefone = input("Telefone: ")

        if not validar_telefone(telefone):

            print("Telefone inválido!")

        else:

            telefone = ''.join(filter(str.isdigit, telefone))
            break

    endereco = input("Endereço: ").strip()

    tipo_sanguineo = input("Tipo sanguíneo: ").upper()

    sql = """
    INSERT INTO pacientes (

        cpf,
        nome,
        idade,
        sexo,
        telefone,
        endereco,
        tipo_sanguineo

    )

    VALUES (%s,%s,%s,%s,%s,%s,%s)
    """

    valores = (

        cpf,
        nome,
        idade,
        sexo,
        telefone,
        endereco,
        tipo_sanguineo

    )

    cursor.execute(sql, valores)

    conexao.commit()

    print("\nPaciente cadastrado!\n")

# ============================================================
# ANAMNESE
# ============================================================

def cadastrar_anamnese():

    global medico_logado

    print("\n========== ANAMNESE ==========\n")

    cpf = input("CPF do paciente: ")

    sql = "SELECT * FROM pacientes WHERE cpf = %s"

    cursor.execute(sql, (cpf,))

    resultado = cursor.fetchone()

    if not resultado:

        print("\nPaciente não encontrado!\n")
        return

    possui_doenca = input("Possui doença? (SIM/NAO): ").upper()

    doencas = ""

    if possui_doenca == "SIM":

        doencas = formatar_doencas(input("Doenças: "))

    observacoes = input("Observações: ").strip()

    sql = """
    INSERT INTO anamneses (

        cpf_paciente,
        rm_medico,
        possui_doenca,
        doencas,
        observacoes

    )

    VALUES (%s,%s,%s,%s,%s)
    """

    valores = (

        cpf,
        medico_logado,
        possui_doenca,
        doencas,
        observacoes

    )

    cursor.execute(sql, valores)

    conexao.commit()

    print("\nAnamnese salva!\n")

# ============================================================
# GERAR LAUDO IA
# ============================================================

def gerar_laudo():

    global medico_logado

    print("\n========== IA CLÍNICA ==========\n")

    cpf = input("CPF do paciente: ")

    sql = "SELECT * FROM pacientes WHERE cpf = %s"

    cursor.execute(sql, (cpf,))

    resultado_paciente = cursor.fetchone()

    if not resultado_paciente:

        print("\nPaciente não encontrado!\n")
        return

    idade = float(input("Idade: "))
    psa_total = float(input("PSA Total: "))
    psa_densidade = float(input("Densidade PSA: "))
    psa_livre = float(input("PSA Livre: "))
    relacao_LT = float(input("Relação L/T (%): "))

    entrada = pd.DataFrame([[

        psa_total,
        psa_livre,
        relacao_LT,
        idade,
        psa_densidade

    ]], columns=X.columns)

    resultado = clf_calibrated.predict(entrada)[0]

    probabilidade = clf_calibrated.predict_proba(entrada)[0]

    status = ""

    if resultado == 1:

        status = "SUSPEITO (BióPSIA)"

    else:

        status = "BENIGNO (MONITORAR)"

    confianca = probabilidade[resultado] * 100

    print("\n========== RESULTADO ==========\n")

    print(f"Diagnóstico: {status}")

    print(f"Confiança: {confianca:.2f}%")

    sql = """
    INSERT INTO laudos (

        cpf_paciente,
        rm_medico,
        idade,
        psa_total,
        psa_livre,
        psa_densidade,
        relacao_lt,
        classificacao_risco,
        resultado,
        observacoes

    )

    VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s)
    """

    valores = (

        cpf,
        medico_logado,
        idade,
        psa_total,
        psa_livre,
        psa_densidade,
        relacao_LT,
        status,
        f"{confianca:.2f}%",
        "Laudo gerado automaticamente pela IA"

    )

    cursor.execute(sql, valores)

    conexao.commit()

    print("\nLaudo salvo!\n")

# ============================================================
# LISTAR LAUDOS
# ============================================================

def listar_laudos():

    sql = "SELECT * FROM laudos"

    cursor.execute(sql)

    dados = cursor.fetchall()

    print("\n========== LAUDOS ==========\n")

    for linha in dados:

        print(linha)

# ============================================================
# MENU
# ============================================================

while True:

    print("\n================ MENU ================\n")

    if medico_logado is None:

        print("1 - Cadastrar médico")
        print("2 - Login")
        print("3 - Sair")

        opcao = input("\nEscolha: ")

        if opcao == "1":

            cadastrar_medico()

        elif opcao == "2":

            login_medico()

        elif opcao == "3":

            break

        else:

            print("\nOpção inválida!")

    else:

        print(f"Médico logado RM: {medico_logado}\n")

        print("1 - Cadastrar paciente")
        print("2 - Cadastrar anamnese")
        print("3 - Gerar laudo IA")
        print("4 - Listar laudos")
        print("5 - Logout")

        opcao = input("\nEscolha: ")

        if opcao == "1":

            cadastrar_paciente()

        elif opcao == "2":

            cadastrar_anamnese()

        elif opcao == "3":

            gerar_laudo()

        elif opcao == "4":

            listar_laudos()

        elif opcao == "5":

            logout()

        else:

            print("\nOpção inválida!")

# ============================================================
# FECHAR
# ============================================================

cursor.close()

conexao.close()

print("\nSistema encerrado!")