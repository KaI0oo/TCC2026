-- ===========================================================
-- BANCO DE DADOS: POSTRATA
-- Script de criação limpa do banco
-- ===========================================================

CREATE DATABASE IF NOT EXISTS postrata
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE postrata;

-- ===========================================================
-- TABELA: MEDICO
-- Armazena usuários do sistema:
-- RH, Médico e Secretária.
-- ===========================================================

CREATE TABLE medico (
    rm INT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    crm VARCHAR(20) NOT NULL,
    senha VARCHAR(100) NOT NULL,
    cargo VARCHAR(20) NOT NULL
);

-- ===========================================================
-- TABELA: PACIENTE
-- Armazena os dados cadastrais dos pacientes
-- e o médico responsável.
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
-- Armazena o histórico clínico do paciente.
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
-- Armazena os dados dos exames utilizados pela IA.
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
-- Armazena o resultado relacionado a um exame.
-- As notas clínicas padrão NÃO são armazenadas,
-- pois são texto fixo da aplicação.
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
-- USUÁRIO INICIAL
-- Primeiro usuário com acesso de RH.
-- ===========================================================

INSERT INTO medico (
    rm,
    nome,
    crm,
    senha,
    cargo
)
VALUES (
    1,
    'Administrador',
    '000000',
    '123',
    'RH'
);

-- ===========================================================
-- USUÁRIO MÉDICO PARA TESTES
-- Pode ser removido na versão final.
-- ===========================================================

INSERT INTO medico (
    rm,
    nome,
    crm,
    senha,
    cargo
)
VALUES (
    2,
    'Dr. João Silva',
    '123456',
    '123',
    'MEDICO'
);

-- ===========================================================
-- CONSULTAS ÚTEIS PARA TESTES
-- ===========================================================

SELECT * FROM funcionario;
SELECT * FROM paciente;
SELECT * FROM anamnese;
SELECT * FROM exame;
SELECT * FROM laudo;

DESCRIBE funcionario;
DESCRIBE paciente;
DESCRIBE anamnese;
DESCRIBE exame;
DESCRIBE laudo;
