CREATE DATABASE postrata;
USE postrata;
SELECT DATABASE();
SELECT @@hostname;

-- ===========================================================
-- TABELA: MEDICO
-- Armazena os profissionais e usuários do sistema
-- (Médico, RH e Secretária).
-- ===========================================================

CREATE TABLE medico (
    rm INT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    crm VARCHAR(20) NOT NULL,
    senha VARCHAR(100) NOT NULL
);

-- ===========================================================
-- TABELA: PACIENTE
-- Armazena os dados cadastrais dos pacientes e o
-- médico responsável pelo acompanhamento.
-- ===========================================================

CREATE TABLE paciente (
    cpf VARCHAR(14) PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    idade INT,
    sexo CHAR(1),
    data_nascimento DATE,
    raca VARCHAR(50),
    telefone VARCHAR(20),
    endereco VARCHAR(200),
    tipo_sanguineo VARCHAR(5),

    rm_medico INT NOT NULL,

    FOREIGN KEY (rm_medico)
    REFERENCES medico(rm)
);

-- ===========================================================
-- TABELA: ANAMNESE
-- Armazena o histórico clínico e informações médicas
-- fornecidas pelo paciente durante a consulta.
-- ===========================================================

CREATE TABLE anamnese (

    id_anamnese INT AUTO_INCREMENT PRIMARY KEY,

    cpf_paciente VARCHAR(14) NOT NULL,

    possui_doenca BOOLEAN,
    doencas TEXT,

    observacoes TEXT,

    toma_remedio BOOLEAN,
    remedio VARCHAR(100),
    dosagem VARCHAR(50),

    inicio_tratamento DATE,
    fim_tratamento DATE,

    tabagismo VARCHAR(20),
    alcool VARCHAR(20),
    frequencia_bebida VARCHAR(100),

    FOREIGN KEY (cpf_paciente)
    REFERENCES paciente(cpf)
);

-- ===========================================================
-- TABELA: EXAME
-- Armazena os resultados dos exames laboratoriais
-- utilizados pela Inteligência Artificial.
-- ===========================================================

CREATE TABLE exame (

    id_exame INT AUTO_INCREMENT PRIMARY KEY,

    cpf_paciente VARCHAR(14) NOT NULL,

    psa_total DECIMAL(10,2),
    psa_livre DECIMAL(10,2),
    densidade_psa DECIMAL(10,2),

    data_exame DATE,

    caminho_pdf VARCHAR(300),

    FOREIGN KEY (cpf_paciente)
    REFERENCES paciente(cpf)
);

-- ===========================================================
-- TABELA: LAUDO
-- Armazena o resultado gerado pelo médico e pela IA
-- com base em um exame previamente cadastrado.
-- ===========================================================

CREATE TABLE laudo (

    id_laudo INT AUTO_INCREMENT PRIMARY KEY,

    id_exame INT NOT NULL,

    classificacao VARCHAR(50),
    interpretacao TEXT,

    data_laudo DATE,

    FOREIGN KEY (id_exame)
    REFERENCES exame(id_exame)
);

-- ===========================================================
-- INSERE O PRIMEIRO USUÁRIO DO SISTEMA
-- Utilizado para realizar o primeiro acesso à aplicação.
-- ===========================================================

INSERT INTO medico
(rm, nome, crm, senha)
VALUES
(1, 'Administrador', '000000', '123');

-- ===========================================================
-- CONSULTAS DE APOIO AO DESENVOLVIMENTO
-- ===========================================================

-- Exibe todos os usuários cadastrados.
SELECT * FROM medico;

-- Exibe apenas os RM cadastrados.
SELECT rm FROM medico;

-- Exibe todos os pacientes cadastrados.
SELECT * FROM paciente;

-- Exibe a estrutura da tabela MEDICO.
DESCRIBE medico;

-- Exibe a estrutura da tabela PACIENTE.
DESCRIBE paciente;

-- ===========================================================
-- COMANDOS UTILIZADOS DURANTE O DESENVOLVIMENTO
-- ===========================================================

-- Remove todos os registros da tabela MEDICO.
DELETE FROM medico;

-- Remove a coluna id_medico da tabela PACIENTE
-- (utilizado apenas durante a modelagem do banco).
ALTER TABLE paciente
DROP COLUMN id_medico;

-- ===========================================================
-- ALTERAÇÕES REALIZADAS APÓS A CRIAÇÃO DO BANCO
-- ===========================================================

-- Adiciona a coluna responsável por identificar o
-- perfil de acesso do usuário (RH, MÉDICO ou SECRETARIA).
ALTER TABLE medico
ADD cargo VARCHAR(20) NOT NULL;

-- Define o primeiro usuário como RH para permitir
-- o cadastro de novos usuários no sistema.
UPDATE medico
SET cargo = 'RH'
WHERE rm = 1;