create database banco_de_dados_prostata;
use  banco_de_dados_prostata;
CREATE TABLE medicos (
    rm INT PRIMARY KEY,
    nome VARCHAR(100),
    especialidade VARCHAR(100),
    telefone VARCHAR(20),
    email VARCHAR(100),
    senha VARCHAR(255)
);
CREATE TABLE pacientes (
    cpf VARCHAR(14) PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    idade INT,
    sexo CHAR(1) NOT NULL,
    data_nascimento DATE,
    raca VARCHAR(50),                   
    telefone VARCHAR(20),
    endereco VARCHAR(200),
    tipo_sanguineo VARCHAR(5),
    data_cadastro DATETIME DEFAULT CURRENT_TIMESTAMP
);
CREATE TABLE anamneses (
    id_anamnese INT AUTO_INCREMENT PRIMARY KEY,
    cpf_paciente VARCHAR(14) NOT NULL,
    rm_medico INT NOT NULL,
    possui_doenca BOOLEAN NOT NULL DEFAULT FALSE,
    doencas TEXT,
    observacoes TEXT,
    toma_remedio BOOLEAN NOT NULL DEFAULT FALSE,
    nome_remedio VARCHAR(100),
    dosagem_mg DECIMAL(6,2),           
    data_inicio_remedio DATE,
    data_fim_remedio DATE,
    fumante ENUM('ATUAL', 'EX-FUMANTE', 'NUNCA') NOT NULL DEFAULT 'NUNCA',
    bebe_alcool ENUM('BEBE', 'EX-BEBEDOR', 'NUNCA') NOT NULL DEFAULT 'NUNCA',
    frequencia_bebida VARCHAR(50),     
    data_registro DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (cpf_paciente) REFERENCES pacientes(cpf) ON DELETE CASCADE,
    FOREIGN KEY (rm_medico) REFERENCES medicos(rm) ON DELETE RESTRICT
);
CREATE TABLE laudos (
    id_laudo INT AUTO_INCREMENT PRIMARY KEY,
    cpf_paciente VARCHAR(14),
    rm_medico INT,
    idade INT,
    psa_total DECIMAL(5,2),
    psa_livre DECIMAL(5,2),
    psa_densidade DECIMAL(5,2),
    relacao_lt DECIMAL(5,2),
    classificacao_risco VARCHAR(100),
    resultado VARCHAR(100),
    observacoes TEXT,
    data_laudo DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (cpf_paciente)
    REFERENCES pacientes(cpf),
    FOREIGN KEY (rm_medico)
    REFERENCES medicos(rm)
);
show tables;
SELECT * FROM pacientes;
SELECT * FROM laudos;
SELECT * FROM anamneses;
SELECT * FROM medicos;
SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS laudos;
DROP TABLE IF EXISTS anamneses;
DROP TABLE IF EXISTS pacientes;
DROP TABLE IF EXISTS medicos;

SET FOREIGN_KEY_CHECKS = 1;
