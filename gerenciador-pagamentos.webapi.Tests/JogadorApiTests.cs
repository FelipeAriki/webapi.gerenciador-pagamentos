using System.Net;
using System.Net.Http.Json;
using System.Text;
using gerenciador_pagamentos.webapi.Service;
using gerenciador_pagamentos.webapi.ViewModel;
using Xunit;

namespace gerenciador_pagamentos.webapi.Tests;

public class JogadorApiTests
{
    private static SalvarJogadorViewModel Dados(decimal pago = 6, decimal pagar = 7) => new()
    {
        Nome = " Felipe ",
        Sobrenome = " Ariki ",
        TotalPago = pago,
        TotalPagar = pagar
    };

    [Fact]
    public async Task CrudCompletoCalculaValoresNormalizaDadosEDevolveStatusCorretos()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var resposta = await client.PostAsJsonAsync("/api/Jogador", Dados());
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var criado = (await resposta.Content.ReadFromJsonAsync<JogadorViewModel>())!;
        Assert.Equal("Felipe", criado.Nome);
        Assert.Equal(1, criado.ValorRestante);
        Assert.False(criado.PagouValorTotal);
        Assert.NotNull(resposta.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(resposta.Headers.Location)).StatusCode);

        var atualizado = await client.PutAsJsonAsync($"/api/Jogador/{criado.Id}", Dados(9, 7));
        var jogador = (await atualizado.Content.ReadFromJsonAsync<JogadorViewModel>())!;
        Assert.Equal(HttpStatusCode.OK, atualizado.StatusCode);
        Assert.True(jogador.PagouValorTotal);
        Assert.Equal(0, jogador.ValorRestante);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/Jogador/{criado.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/Jogador/{criado.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync($"/api/Jogador/{criado.Id}", Dados())).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/Jogador/{criado.Id}")).StatusCode);
    }

    [Theory]
    [InlineData("{\"nome\":\" \",\"sobrenome\":\"Silva\",\"totalPago\":0,\"totalPagar\":10}")]
    [InlineData("{\"nome\":\"Ana\",\"sobrenome\":\"Silva\",\"totalPago\":-1,\"totalPagar\":10}")]
    [InlineData("{\"nome\":\"Ana\",\"sobrenome\":\"Silva\",\"totalPago\":1.001,\"totalPagar\":10}")]
    [InlineData("{\"nome\":\"Ana\",\"sobrenome\":\"Silva\",\"totalPago\":100000000,\"totalPagar\":10}")]
    [InlineData("{\"nome\":\"Ana\",\"sobrenome\":\"Silva\",\"totalPago\":0,\"totalPagar\":10,\"urlImagemComprovante\":\"javascript:alert(1)\"}")]
    [InlineData("{\"nome\":\"Ana\",\"sobrenome\":\"Silva\"}")]
    public async Task DadosInvalidosSaoRejeitadosSemGravar(string json)
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var resposta = await client.PostAsync("/api/Jogador", new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("errors", await resposta.Content.ReadAsStringAsync());
        var pagina = (await client.GetFromJsonAsync<PaginaJogadoresViewModel>("/api/Jogador"))!;
        Assert.Equal(0, pagina.Total);
    }

    [Fact]
    public async Task PaginacaoEFiltroMantemResumoDeTodosOsResultados()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/Jogador", Dados(2, 7));
        await client.PostAsJsonAsync("/api/Jogador", Dados(3, 7));
        await client.PostAsJsonAsync("/api/Jogador", Dados(10, 7));
        var pagina = (await client.GetFromJsonAsync<PaginaJogadoresViewModel>("/api/Jogador?pagina=2&tamanhoPagina=1&situacao=pendente&busca=ari"))!;
        Assert.Single(pagina.Itens);
        Assert.Equal(2, pagina.Total);
        Assert.Equal(2, pagina.TotalPaginas);
        Assert.Equal(5, pagina.Resumo.TotalPago);
        Assert.Equal(9, pagina.Resumo.ValorRestante);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/Jogador?pagina=0")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/Jogador?tamanhoPagina=101")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/Jogador?situacao=errada")).StatusCode);
    }

    [Fact]
    public async Task ErrosInternosNaoVazamDetalhesEHealthResponde()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        factory.Repository.Falhar = true;
        var resposta = await client.GetAsync("/api/Jogador");
        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("traceId", corpo);
        Assert.DoesNotContain("confidencial", corpo);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
    }

    [Fact]
    public async Task ServicoPropagaCancelamentoEPreservaUrlsOpcionais()
    {
        var repository = new MemoryJogadorRepository();
        var service = new JogadorService(repository, repository);
        var dados = Dados();
        dados.UrlImagem = " https://example.com/foto.jpg ";
        dados.UrlImagemComprovante = " ";
        var jogador = await service.CriarJogador(dados);
        Assert.Equal("https://example.com/foto.jpg", jogador.UrlImagem);
        Assert.Null(jogador.UrlImagemComprovante);
        using var source = new CancellationTokenSource();
        await service.ObterDadosJogador(jogador.Id, source.Token);
        Assert.Equal(source.Token, repository.UltimoToken);
        source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ExcluirJogador(jogador.Id, source.Token));
    }
}
