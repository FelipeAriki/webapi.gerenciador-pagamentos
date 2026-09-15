CREATE TABLE IF NOT EXISTS jogador (
    id                     SERIAL PRIMARY KEY,
    url_imagem             VARCHAR(500),
    nome                   VARCHAR(100)  NOT NULL,
    sobrenome              VARCHAR(100)  NOT NULL,
    total_pago             NUMERIC(10,2) NOT NULL DEFAULT 0,
    total_pagar            NUMERIC(10,2) NOT NULL DEFAULT 0,
    url_imagem_comprovante VARCHAR(500),

    CONSTRAINT ck_jogador_total_pago_nao_negativo  CHECK (total_pago  >= 0),
    CONSTRAINT ck_jogador_total_pagar_nao_negativo CHECK (total_pagar >= 0)
);

CREATE INDEX IF NOT EXISTS ix_jogador_nome ON jogador (nome, sobrenome);

INSERT INTO jogador (nome, sobrenome, total_pago, total_pagar)
SELECT 'Felipe', 'Ariki', 6.00, 7.00
WHERE NOT EXISTS (
    SELECT 1 FROM jogador WHERE nome = 'Felipe' AND sobrenome = 'Ariki'
);
