using System.Data;
using Dapper;
using gerenciador_pagamentos.webapi.Interface;
using gerenciador_pagamentos.webapi.Model;
using gerenciador_pagamentos.webapi.ViewModel;
using Npgsql;

namespace gerenciador_pagamentos.webapi.Repository;

public class JogadorQueryRepository(NpgsqlDataSource dataSource) : IJogadorQueryRepository
{
    private const string Filtro = """
        WHERE (@Busca IS NULL OR nome || ' ' || sobrenome ILIKE @Busca ESCAPE '\')
          AND (@Situacao = 'todos'
            OR (@Situacao = 'quitado' AND total_pago >= total_pagar)
            OR (@Situacao = 'pendente' AND total_pago < total_pagar))
        """;

    public async Task<(IReadOnlyList<Jogador> Itens, ResumoPagamentosViewModel Resumo)> ObterDadosJogadores(
        ConsultaJogadoresViewModel consulta, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT " + JogadorSql.Colunas + " FROM jogador " + Filtro + "\n" + """
            ORDER BY nome, sobrenome, id LIMIT @Limite OFFSET @Offset;
            SELECT COUNT(*) AS TotalJogadores,
                COUNT(*) FILTER (WHERE total_pago >= total_pagar) AS Quitados,
                COUNT(*) FILTER (WHERE total_pago < total_pagar) AS Pendentes,
                COALESCE(SUM(total_pago), 0) AS TotalPago,
                COALESCE(SUM(total_pagar), 0) AS TotalPagar,
                COALESCE(SUM(GREATEST(total_pagar - total_pago, 0)), 0) AS ValorRestante
            FROM jogador
            """ + "\n" + Filtro + ";";
        var busca = consulta.Busca?.Trim();
        var parametros = new
        {
            Busca = string.IsNullOrEmpty(busca) ? null : "%" + busca.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%",
            consulta.Situacao,
            Limite = consulta.TamanhoPagina,
            Offset = (consulta.Pagina - 1) * consulta.TamanhoPagina
        };
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        // Itens e resumo representam a mesma fotografia, mesmo durante outros cadastros.
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
        using var resultados = await connection.QueryMultipleAsync(new CommandDefinition(sql, parametros, transaction, cancellationToken: cancellationToken));
        var itens = (await resultados.ReadAsync<Jogador>()).ToList();
        var resumo = await resultados.ReadSingleAsync<ResumoPagamentosViewModel>();
        await transaction.CommitAsync(cancellationToken);
        return (itens, resumo);
    }

    public async Task<Jogador?> ObterDadosJogador(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT " + JogadorSql.Colunas + " FROM jogador WHERE id = @Id;";
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Jogador>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }
}
