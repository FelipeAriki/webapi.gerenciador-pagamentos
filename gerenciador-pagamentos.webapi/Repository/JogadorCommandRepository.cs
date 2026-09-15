using Dapper;
using gerenciador_pagamentos.webapi.Interface;
using Npgsql;
using System.Data;

namespace gerenciador_pagamentos.webapi.Repository;

public class JogadorCommandRepository : IJogadorCommandRepository
{
    private readonly string _connectionString;
    public JogadorCommandRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<bool> ExcluirJogador(int id)
    {
        const string sql = "DELETE FROM jogador WHERE id = @Id;";
        using var connection = CreateConnection();
        var excluiu = await connection.ExecuteAsync(sql, new { Id = id });
        return excluiu > 0;
    }
}
