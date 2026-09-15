using Dapper;
using gerenciador_pagamentos.webapi.Interface;
using gerenciador_pagamentos.webapi.Model;
using Npgsql;
using System.Data;

namespace gerenciador_pagamentos.webapi.Repository;

public class JogadorQueryRepository : IJogadorQueryRepository
{
    private readonly string _connectionString;

    public JogadorQueryRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    private const string Colunas = @"
                id                     AS Id,
                url_imagem             AS UrlImagem,
                nome                   AS Nome,
                sobrenome              AS Sobrenome,
                total_pago             AS TotalPago,
                total_pagar            AS TotalPagar,
                url_imagem_comprovante AS UrlImagemComprovante";

    private const string SqlListarTodos =
        "SELECT " + Colunas + @"
            FROM jogador
            ORDER BY nome, sobrenome;";

    private const string SqlObterPorId =
        "SELECT " + Colunas + @"
            FROM jogador
            WHERE id = @Id;";

    public async Task<IEnumerable<Jogador>> ObterDadosJogadores()
    {
        using var connection = CreateConnection();
        return await connection.QueryAsync<Jogador>(SqlListarTodos);
    }

    public async Task<Jogador?> ObterDadosJogador(int id)
    {
        using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Jogador>(SqlObterPorId, new { Id = id });
    }
}
