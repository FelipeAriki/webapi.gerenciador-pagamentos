using System.ComponentModel.DataAnnotations;
using gerenciador_pagamentos.webapi.Interface;
using gerenciador_pagamentos.webapi.Model;
using gerenciador_pagamentos.webapi.ViewModel;

namespace gerenciador_pagamentos.webapi.Service;

public class JogadorService(IJogadorQueryRepository queryRepository, IJogadorCommandRepository commandRepository) : IJogadorService
{
    public async Task<PaginaJogadoresViewModel> ObterDadosJogadores(ConsultaJogadoresViewModel consulta, CancellationToken cancellationToken = default)
    {
        Validator.ValidateObject(consulta, new ValidationContext(consulta), true);
        var (itens, resumo) = await queryRepository.ObterDadosJogadores(consulta, cancellationToken);
        return new PaginaJogadoresViewModel
        {
            Itens = itens.Select(MapearParaViewModel).ToList(),
            Resumo = resumo,
            Pagina = consulta.Pagina,
            TamanhoPagina = consulta.TamanhoPagina
        };
    }

    public async Task<JogadorViewModel?> ObterDadosJogador(int id, CancellationToken cancellationToken = default)
    {
        var jogador = await queryRepository.ObterDadosJogador(id, cancellationToken);
        return jogador is null ? null : MapearParaViewModel(jogador);
    }

    public async Task<JogadorViewModel> CriarJogador(SalvarJogadorViewModel dados, CancellationToken cancellationToken = default)
    {
        var jogador = await commandRepository.CriarJogador(MapearParaModel(dados), cancellationToken);
        return MapearParaViewModel(jogador);
    }

    public async Task<JogadorViewModel?> AlterarJogador(int id, SalvarJogadorViewModel dados, CancellationToken cancellationToken = default)
    {
        var jogador = MapearParaModel(dados);
        jogador.Id = id;
        var atualizado = await commandRepository.AlterarJogador(jogador, cancellationToken);
        return atualizado is null ? null : MapearParaViewModel(atualizado);
    }

    public Task<bool> ExcluirJogador(int id, CancellationToken cancellationToken = default) =>
        commandRepository.ExcluirJogador(id, cancellationToken);

    private static Jogador MapearParaModel(SalvarJogadorViewModel dados)
    {
        Validator.ValidateObject(dados, new ValidationContext(dados), true);
        return new Jogador
        {
            Nome = dados.Nome.Trim(),
            Sobrenome = dados.Sobrenome.Trim(),
            TotalPago = dados.TotalPago!.Value,
            TotalPagar = dados.TotalPagar!.Value,
            UrlImagem = NormalizarUrl(dados.UrlImagem),
            UrlImagemComprovante = NormalizarUrl(dados.UrlImagemComprovante)
        };
    }

    private static string? NormalizarUrl(string? url) => string.IsNullOrWhiteSpace(url) ? null : url.Trim();

    private static JogadorViewModel MapearParaViewModel(Jogador jogador) => new()
    {
        Id = jogador.Id,
        UrlImagem = jogador.UrlImagem,
        Nome = jogador.Nome,
        Sobrenome = jogador.Sobrenome,
        TotalPago = jogador.TotalPago,
        TotalPagar = jogador.TotalPagar,
        UrlImagemComprovante = jogador.UrlImagemComprovante,
        PagouValorTotal = jogador.TotalPago >= jogador.TotalPagar,
        ValorRestante = Math.Max(0m, jogador.TotalPagar - jogador.TotalPago)
    };
}
