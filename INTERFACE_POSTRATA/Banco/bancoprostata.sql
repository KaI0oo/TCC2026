-- ===========================================================
-- BANCO DE DADOS: postrata
-- Recriação limpa (DROP + CREATE)
-- Login por nome de usuário (coluna usuario).
-- rm permanece como ID interno (AUTO_INCREMENT / FK).
-- ===========================================================



CREATE DATABASE postrata
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE postrata;

-- ===========================================================
-- TABELA: FUNCIONARIO
-- ===========================================================

CREATE TABLE funcionario (
    rm INT AUTO_INCREMENT PRIMARY KEY,
    usuario VARCHAR(50) NOT NULL,
    nome VARCHAR(100) NOT NULL,
    crm VARCHAR(20),
    senha VARCHAR(100) NOT NULL,
    cargo VARCHAR(20) NOT NULL,
    UNIQUE KEY uk_funcionario_usuario (usuario),
    UNIQUE KEY idx_crm_unique (crm)
);

-- ===========================================================
-- TABELA: PACIENTE
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
        REFERENCES funcionario(rm)
);

-- ===========================================================
-- TABELA: ANAMNESE
-- Colunas alinhadas ao INSERT da aplicação.
-- ===========================================================

CREATE TABLE anamnese (
    id_anamnese INT AUTO_INCREMENT PRIMARY KEY,

    cpf_paciente VARCHAR(14) NOT NULL,
    rm_medico INT NULL,

    possui_doenca BOOLEAN,
    doencas TEXT,

    observacoes TEXT,

    toma_remedio BOOLEAN,
    remedio_nome VARCHAR(100),
    dosagem_mg VARCHAR(50),

    inicio_tratamento DATE,
    fim_tratamento DATE,

    tabagismo VARCHAR(20),
    alcool VARCHAR(20),
    frequencia VARCHAR(100),

    FOREIGN KEY (cpf_paciente)
        REFERENCES paciente(cpf),
    FOREIGN KEY (rm_medico)
        REFERENCES funcionario(rm)
);

-- ===========================================================
-- TABELA: EXAME
-- ===========================================================

CREATE TABLE exame (
    id_exame INT AUTO_INCREMENT PRIMARY KEY,

    cpf_paciente VARCHAR(14) NOT NULL,

    psa_total DECIMAL(10,2) NOT NULL,
    psa_livre DECIMAL(10,2) NOT NULL,
    densidade_psa DECIMAL(10,2),

    data_exame DATE,

    caminho_pdf VARCHAR(300),

    FOREIGN KEY (cpf_paciente)
        REFERENCES paciente(cpf)
);

-- ===========================================================
-- TABELA: LAUDO
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
-- USUÁRIO INICIAL (RH)
-- Login: admin / 123
-- ===========================================================

INSERT INTO funcionario (
    usuario,
    nome,
    crm,
    senha,
    cargo
)
VALUES (
    'admin',
    'Administrador',
    '000000',
    '123',
    'RH'
);

-- ===========================================================
-- USUÁRIO MÉDICO PARA TESTES
-- Login: joao.silva / 123
-- ===========================================================

INSERT INTO funcionario (
    usuario,
    nome,
    crm,
    senha,
    cargo
)
VALUES (
    'joao.silva',
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
