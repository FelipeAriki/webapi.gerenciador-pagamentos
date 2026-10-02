namespace gerenciador_pagamentos.webapi.Repository;

internal static class JogadorSql
{
    internal const string Colunas = """
        id AS Id, url_imagem AS UrlImagem, nome AS Nome, sobrenome AS Sobrenome,
        total_pago AS TotalPago, total_pagar AS TotalPagar,
        url_imagem_comprovante AS UrlImagemComprovante
        """;
}
