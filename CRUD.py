import pandas as pd
import mysql.connector
from datetime import datetime
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

emails_validos = ["@gmail.com", "@hotmail.com", "@outlook.com", "@yahoo.com"]

# ============================================================
# CONEXÃO MYSQL
# ============================================================
conexao = mysql.connector.connect(
    host=host, user=user, password=password, database=database
)
cursor = conexao.cursor()
print("\nBanco conectado com sucesso!\n")

# ============================================================
# IA - TREINAMENTO
# ============================================================
df = pd.read_csv("dados_psa_clinica.csv")
X = df.drop('Resultado', axis=1)
y = df['Resultado']

X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.3, random_state=42)

clf = DecisionTreeClassifier(max_depth=3, criterion='entropy', random_state=42)
clf.fit(X_train, y_train)
clf_calibrated = CalibratedClassifierCV(clf, cv=5, method='sigmoid')
clf_calibrated.fit(X_train, y_train)

# ============================================================
# FUNÇÕES AUXILIARES
# ============================================================
def validar_cpf(cpf):
    cpf = ''.join(filter(str.isdigit, cpf))
    return len(cpf) == 11

def validar_tipo_sanguineo(tipo):
    tipos_validos = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"]
    return tipo.upper() in tipos_validos

def calcular_idade(data_nascimento):
    try:
        nasc = datetime.strptime(data_nascimento, "%d-%m-%Y")
        hoje = datetime.today()
        idade = hoje.year - nasc.year - ((hoje.month, hoje.day) < (nasc.month, nasc.day))
        return idade
    except:
        return None

def formatar_telefone(telefone):
    digitos = ''.join(filter(str.isdigit, telefone))
    if len(digitos) in [10, 11]:
        return f"+55{digitos}"
    return None

def formatar_nome(nome):
    return nome.strip().title()

def formatar_doencas(doencas):
    lista = [doenca.strip().capitalize() for doenca in doencas.split(",")]
    return ", ".join(lista)

# ============================================================
# CADASTRO E LOGIN MÉDICO
# ============================================================
def cadastrar_medico():
    global medico_logado
    print("\n========== CADASTRO MÉDICO ==========\n")
    rm = input("RM: ").strip()
    if not rm.isdigit():
        print("RM inválido!"); return

    cursor.execute("SELECT * FROM medicos WHERE rm = %s", (rm,))
    if cursor.fetchone():
        print("\nRM já cadastrado!\n"); return

    nome = formatar_nome(input("Nome: "))
    especialidade = formatar_nome(input("Especialidade: "))
    
    telefone = input("Telefone: ")
    while not (tel_formatado := formatar_telefone(telefone)):
        print("Telefone inválido!"); telefone = input("Telefone: ")

    email = input("Email: ").strip().lower()
    while not any(email.endswith(x) for x in emails_validos):
        print("Email inválido!"); email = input("Email: ").strip().lower()

    senha = input("Senha: ").strip()
    while len(senha) < 4:
        print("Senha muito curta!"); senha = input("Senha: ").strip()

    sql = """INSERT INTO medicos (rm, nome, especialidade, telefone, email, senha)
             VALUES (%s, %s, %s, %s, %s, %s)"""
    cursor.execute(sql, (rm, nome, especialidade, tel_formatado, email, senha))
    conexao.commit()
    print("\nMédico cadastrado com sucesso!\n")


def login_medico():
    global medico_logado
    print("\n========== LOGIN ==========\n")
    rm = input("RM: ")
    senha = input("Senha: ")
    cursor.execute("SELECT * FROM medicos WHERE rm = %s AND senha = %s", (rm, senha))
    if cursor.fetchone():
        medico_logado = rm
        print("\nLogin realizado com sucesso!\n")
    else:
        print("\nRM ou senha inválidos!\n")


def logout():
    global medico_logado
    medico_logado = None
    print("\nLogout realizado!\n")

# ============================================================
# PACIENTES
# ============================================================
def cadastrar_paciente():
    print("\n========== CADASTRO PACIENTE ==========\n")
    cpf = input("CPF: ")
    while not validar_cpf(cpf):
        print("CPF inválido!"); cpf = input("CPF: ")

    cursor.execute("SELECT * FROM pacientes WHERE cpf = %s", (cpf,))
    if cursor.fetchone():
        print("\nCPF já cadastrado!\n"); return

    nome = formatar_nome(input("Nome: "))
    while True:
        data_nasc = input("Data de Nascimento (DD-MM-AAAA): ").strip()
        idade = calcular_idade(data_nasc)
        if idade is not None: break
        print("Data inválida!")

    sexo = input("Sexo (M/F): ").upper()
    while sexo not in ["M", "F"]: sexo = input("Sexo (M/F): ").upper()

    raca = input("Raça/Cor: ").strip().title()
    telefone = input("Telefone: ")
    while not (tel_formatado := formatar_telefone(telefone)):
        print("Telefone inválido!"); telefone = input("Telefone: ")

    endereco = input("Endereço: ").strip()
    while True:
        tipo_sanguineo = input("Tipo sanguíneo (ex: A+): ").strip().upper()
        if validar_tipo_sanguineo(tipo_sanguineo): break
        print("Tipo inválido!")

    sql = """INSERT INTO pacientes 
    (cpf, nome, idade, sexo, data_nascimento, raca, telefone, endereco, tipo_sanguineo)
    VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s)"""
    cursor.execute(sql, (cpf, nome, idade, sexo, data_nasc, raca, tel_formatado, endereco, tipo_sanguineo))
    conexao.commit()
    print(f"\nPaciente cadastrado com sucesso! (Idade: {idade} anos)\n")

def alterar_paciente():
    print("\n========== ALTERAR PACIENTE ==========\n")
    cpf = input("CPF do paciente: ").strip()
    
    cursor.execute("SELECT * FROM pacientes WHERE cpf = %s", (cpf,))
    paciente = cursor.fetchone()
    if not paciente:
        print("Paciente não encontrado!")
        return

    print("Deixe em branco para manter o valor atual.\n")
    
    nome = input(f"Nome ({paciente[1]}): ").strip()
    nome = formatar_nome(nome) if nome else paciente[1]

    data_nasc_input = input(f"Data Nascimento ({paciente[4] or ''}): ").strip()
    if data_nasc_input:
        idade = calcular_idade(data_nasc_input)
        data_nasc = data_nasc_input if idade is not None else paciente[4]
        idade = idade if idade is not None else paciente[2]
    else:
        data_nasc = paciente[4]
        idade = paciente[2]

    raca = input(f"Raça ({paciente[5] or 'Não informada'}): ").strip().title() or (paciente[5] or "")

    tel_atual = paciente[6] if len(paciente) > 6 else ""
    telefone = input(f"Telefone ({tel_atual}): ").strip()
    if telefone:
        tel_formatado = formatar_telefone(telefone)
        telefone = tel_formatado if tel_formatado else tel_atual
    else:
        telefone = tel_atual

    endereco = input(f"Endereço ({paciente[7] if len(paciente) > 7 else ''}): ").strip() or (paciente[7] if len(paciente) > 7 else "")

    tipo_atual = paciente[8] if len(paciente) > 8 else ""
    tipo_sanguineo = input(f"Tipo sanguíneo ({tipo_atual}): ").strip().upper()
    if tipo_sanguineo:
        if not validar_tipo_sanguineo(tipo_sanguineo):
            print("Tipo sanguíneo inválido! Mantendo atual.")
            tipo_sanguineo = tipo_atual
    else:
        tipo_sanguineo = tipo_atual

    sql = """
    UPDATE pacientes 
    SET nome=%s, idade=%s, data_nascimento=%s, raca=%s, telefone=%s, 
        endereco=%s, tipo_sanguineo=%s
    WHERE cpf=%s
    """
    cursor.execute(sql, (nome, idade, data_nasc, raca, telefone, endereco, tipo_sanguineo, cpf))
    conexao.commit()
    print("\nPaciente atualizado com sucesso!\n")

def listar_pacientes_medico():
    print("\n========== MEUS PACIENTES ==========\n")
    cursor.execute("SELECT * FROM pacientes ORDER BY nome")
    pacientes = cursor.fetchall()
    if not pacientes:
        print("Nenhum paciente cadastrado.")
        return
    for p in pacientes:
        print(f"CPF: {p[0]} | Nome: {p[1]} | Idade: {p[2]} | Raça: {p[5] or '-'} | Tipo: {p[8] or '-'}")
    print()


# ============================================================
# ANAMNESE (com SIM/NAO para Fuma e Bebe)
# ============================================================
def cadastrar_anamnese():
    print("\n========== CADASTRO DE ANAMNESE ==========\n")
    cpf = input("CPF do paciente: ")
    cursor.execute("SELECT * FROM pacientes WHERE cpf = %s", (cpf,))
    if not cursor.fetchone():
        print("Paciente não encontrado!"); return

    possui_doenca = input("Possui alguma doença? (SIM/NAO): ").upper() == "SIM"
    doencas = formatar_doencas(input("Quais doenças? (separadas por vírgula): ")) if possui_doenca else ""

    # Remédio
    toma_remedio = input("Toma algum medicamento? (SIM/NAO): ").upper() == "SIM"
    nome_remedio = dosagem = data_inicio = data_fim = None
    if toma_remedio:
        nome_remedio = input("Nome do medicamento: ").strip()
        dosagem = float(input("Dosagem (mg): ") or 0)
        data_inicio = input("Data de início (DD-MM-AAAA): ") or None
        data_fim = input("Data de término (DD-MM-AAAA): ") or None

    # Fuma
    fuma = input("Fuma atualmente? (SIM/NAO): ").upper() == "SIM"
    fumante = input("Status do fumo (ATUAL / EX-FUMANTE / NUNCA): ").upper() if fuma else "NUNCA"
    if fumante not in ['ATUAL', 'EX-FUMANTE', 'NUNCA']: fumante = 'NUNCA'

    # Bebe
    bebe = input("Bebe álcool? (SIM/NAO): ").upper() == "SIM"
    bebe_alcool = input("Status (BEBE / EX-BEBEDOR / NUNCA): ").upper() if bebe else "NUNCA"
    if bebe_alcool not in ['BEBE', 'EX-BEBEDOR', 'NUNCA']: bebe_alcool = 'NUNCA'
    frequencia_bebida = input("Frequência (ex: 2x por semana): ").strip() if bebe else ""

    observacoes = input("Observações: ").strip()

    sql = """
    INSERT INTO anamneses 
    (cpf_paciente, rm_medico, possui_doenca, doencas, observacoes, 
     toma_remedio, nome_remedio, dosagem_mg, data_inicio_remedio, data_fim_remedio,
     fumante, bebe_alcool, frequencia_bebida)
    VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
    """
    valores = (cpf, medico_logado, possui_doenca, doencas, observacoes,
               toma_remedio, nome_remedio, dosagem, data_inicio, data_fim,
               fumante, bebe_alcool, frequencia_bebida)
    
    cursor.execute(sql, valores)
    conexao.commit()
    print("\nAnamnese salva com sucesso!\n")


# ============================================================
# GERAR LAUDO IA (Corrigido)
# ============================================================
def gerar_laudo():
    print("\n========== IA CLÍNICA ==========\n")
    cpf = input("CPF do paciente: ").strip()
    cursor.execute("SELECT * FROM pacientes WHERE cpf = %s", (cpf,))
    if not cursor.fetchone():
        print("Paciente não encontrado!")
        return

    try:
        idade = float(input("Idade: "))
        psa_total = float(input("PSA Total: "))
        psa_densidade = float(input("Densidade PSA: "))
        psa_livre = float(input("PSA Livre: "))
        relacao_LT = float(input("Relação L/T (%): "))

        entrada = pd.DataFrame([[psa_total, psa_livre, relacao_LT, idade, psa_densidade]], columns=X.columns)

        resultado = clf_calibrated.predict(entrada)[0]
        probabilidade = clf_calibrated.predict_proba(entrada)[0]
        
        status = "SUSPEITO (BióPSIA)" if resultado == 1 else "BENIGNO (MONITORAR)"
        confianca = probabilidade[resultado] * 100

        print("\n" + "="*50)
        print(f"Diagnóstico: {status}")
        print(f"Confiança: {confianca:.2f}%")
        print("="*50)

        sql = """
        INSERT INTO laudos 
        (cpf_paciente, rm_medico, idade, psa_total, psa_livre, psa_densidade, 
         relacao_lt, classificacao_risco, resultado, observacoes)
        VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s)
        """
        valores = (cpf, medico_logado, idade, psa_total, psa_livre, psa_densidade,
                   relacao_LT, status, f"{confianca:.2f}%", "Laudo gerado pela IA")
        cursor.execute(sql, valores)
        conexao.commit()
        print("\nLaudo salvo com sucesso!\n")

    except Exception as e:
        print(f"\nErro ao gerar laudo: {e}")


def listar_laudos_medico():
    print("\n========== MEUS LAUDOS ==========\n")
    cursor.execute("""SELECT l.data_laudo, p.nome, l.resultado 
                      FROM laudos l JOIN pacientes p ON l.cpf_paciente = p.cpf 
                      WHERE l.rm_medico = %s ORDER BY l.data_laudo DESC""", (medico_logado,))
    for row in cursor.fetchall():
        print(f"Data: {row[0]} | Paciente: {row[1]} | Resultado: {row[2]}")


def listar_laudos_paciente():
    cpf = input("\nDigite o CPF do paciente: ").strip()
    print(f"\n========== LAUDOS DO PACIENTE {cpf} ==========\n")
    cursor.execute("""SELECT data_laudo, resultado 
                      FROM laudos WHERE cpf_paciente = %s AND rm_medico = %s 
                      ORDER BY data_laudo DESC""", (cpf, medico_logado))
    for row in cursor.fetchall():
        print(f"Data: {row[0]} | Resultado: {row[1]}")


# ============================================================
# MENU PRINCIPAL
# ============================================================
while True:
    print("\n" + "="*60)
    if medico_logado is None:
        print("1 - Cadastrar médico")
        print("2 - Login")
        print("3 - Sair")
        opcao = input("\nEscolha: ")
        if opcao == "1": cadastrar_medico()
        elif opcao == "2": login_medico()
        elif opcao == "3": break
        else: print("Opção inválida!")
    else:
        print(f"Médico logado - RM: {medico_logado}\n")
        print("1 - Cadastrar paciente")
        print("2 - Alterar paciente")
        print("3 - Cadastrar anamnese")
        print("4 - Gerar laudo IA")
        print("5 - Meus pacientes")
        print("6 - Meus laudos")
        print("7 - Laudos de um paciente")
        print("8 - Logout")
        opcao = input("\nEscolha: ")

        if opcao == "1": cadastrar_paciente()
        elif opcao == "2": alterar_paciente()  # Use a função que você já tem
        elif opcao == "3": cadastrar_anamnese()
        elif opcao == "4": gerar_laudo()
        elif opcao == "5": listar_pacientes_medico()
        elif opcao == "6": listar_laudos_medico()
        elif opcao == "7": listar_laudos_paciente()
        elif opcao == "8": logout()
        else: print("Opção inválida!")

# ============================================================
# FECHAR CONEXÃO
# ============================================================
cursor.close()
conexao.close()
print("\nSistema encerrado!")
