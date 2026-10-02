using gerenciador_pagamentos.webapi.Model;
using gerenciador_pagamentos.webapi.Repository;
using gerenciador_pagamentos.webapi.ViewModel;
using Npgsql;
using Xunit;

namespace gerenciador_pagamentos.webapi.Tests;

public class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION_STRING")))
            Skip = "Configure TEST_POSTGRES_CONNECTION_STRING para validar os SQLs em um PostgreSQL local de testes.";
    }
}

public class PostgresRepositoryTests
{
    [PostgresFact]
    public async Task CrudPaginacaoResumoEBuscaLiteralFuncionamNoPostgres()
    {
        var conexao = Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION_STRING")!;
        // Identificador gerado internamente; nunca utiliza um nome fornecido pelo usuário.
        var schema = "gerenciador_test_" + Guid.NewGuid().ToString("N");
        await using var admin = NpgsqlDataSource.Create(conexao);
        await using (var criarSchema = admin.CreateCommand($"CREATE SCHEMA {schema}"))
            await criarSchema.ExecuteNonQueryAsync();
        try
        {
            var configuracao = new NpgsqlConnectionStringBuilder(conexao) { SearchPath = schema };
            await using var dataSource = NpgsqlDataSource.Create(configuracao.ConnectionString);
            var script = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Database", "001_create_jogador.sql"));
            await using (var criarTabela = dataSource.CreateCommand(script))
                await criarTabela.ExecuteNonQueryAsync();
            var comandos = new JogadorCommandRepository(dataSource);
            var consultas = new JogadorQueryRepository(dataSource);
            var ana = await comandos.CriarJogador(new Jogador { Nome = "Ana%", Sobrenome = "Teste", TotalPago = 12.50m, TotalPagar = 50m });
            var bruno = await comandos.CriarJogador(new Jogador { Nome = "Bruno", Sobrenome = "Teste", TotalPago = 60m, TotalPagar = 50m });
            Assert.Equal(12.50m, (await consultas.ObterDadosJogador(ana.Id))!.TotalPago);

            var (itens, resumo) = await consultas.ObterDadosJogadores(new ConsultaJogadoresViewModel { Busca = "Teste", TamanhoPagina = 1 });
            Assert.Single(itens);
            Assert.Equal(2, resumo.TotalJogadores);
            Assert.Equal(72.50m, resumo.TotalPago);
            Assert.Equal(37.50m, resumo.ValorRestante);
            var (segundaPagina, _) = await consultas.ObterDadosJogadores(new ConsultaJogadoresViewModel { Busca = "Teste", TamanhoPagina = 1, Pagina = 2 });
            Assert.Equal(bruno.Id, Assert.Single(segundaPagina).Id);
            var (buscaLiteral, _) = await consultas.ObterDadosJogadores(new ConsultaJogadoresViewModel { Busca = "%" });
            Assert.Equal(ana.Id, Assert.Single(buscaLiteral).Id);
            var (quitados, _) = await consultas.ObterDadosJogadores(new ConsultaJogadoresViewModel { Busca = "Teste", Situacao = "quitado" });
            Assert.Equal(bruno.Id, Assert.Single(quitados).Id);
            // Busca nula também exercita o parâmetro SQL sem texto informado.
            var (_, todos) = await consultas.ObterDadosJogadores(new ConsultaJogadoresViewModel());
            Assert.Equal(3, todos.TotalJogadores); // Inclui o exemplo do script.

            ana.TotalPago = 50m;
            Assert.Equal(50m, (await comandos.AlterarJogador(ana))!.TotalPago);
            Assert.True(await comandos.ExcluirJogador(ana.Id));
            Assert.Null(await consultas.ObterDadosJogador(ana.Id));
            Assert.Null(await comandos.AlterarJogador(ana));
            Assert.False(await comandos.ExcluirJogador(ana.Id));
        }
        finally
        {
            // Remove somente o schema descartável criado por este teste.
            await using var removerSchema = admin.CreateCommand($"DROP SCHEMA {schema} CASCADE");
            await removerSchema.ExecuteNonQueryAsync();
        }
    }
}
