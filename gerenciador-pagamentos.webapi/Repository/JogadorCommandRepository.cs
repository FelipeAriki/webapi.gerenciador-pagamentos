using Dapper;
using gerenciador_pagamentos.webapi.Interface;
using gerenciador_pagamentos.webapi.Model;
using Npgsql;

namespace gerenciador_pagamentos.webapi.Repository;

public class JogadorCommandRepository(NpgsqlDataSource dataSource) : IJogadorCommandRepository
{
    public async Task<Jogador> CriarJogador(Jogador jogador, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO jogador (nome, sobrenome, total_pago, total_pagar, url_imagem, url_imagem_comprovante)
            VALUES (@Nome, @Sobrenome, @TotalPago, @TotalPagar, @UrlImagem, @UrlImagemComprovante)
            RETURNING
            """ + " " + JogadorSql.Colunas;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<Jogador>(new CommandDefinition(sql, jogador, cancellationToken: cancellationToken));
    }

    public async Task<Jogador?> AlterarJogador(Jogador jogador, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE jogador SET nome = @Nome, sobrenome = @Sobrenome,
                total_pago = @TotalPago, total_pagar = @TotalPagar,
                url_imagem = @UrlImagem, url_imagem_comprovante = @UrlImagemComprovante
            WHERE id = @Id
            RETURNING
            """ + " " + JogadorSql.Colunas;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Jogador>(new CommandDefinition(sql, jogador, cancellationToken: cancellationToken));
    }

    public async Task<bool> ExcluirJogador(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM jogador WHERE id = @Id;";
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken)) > 0;
    }
}
