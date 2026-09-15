using gerenciador_pagamentos.webapi.Interface;
using gerenciador_pagamentos.webapi.Model;
using gerenciador_pagamentos.webapi.ViewModel;

namespace gerenciador_pagamentos.webapi.Service;

public class JogadorService : IJogadorService
{
    private readonly IJogadorQueryRepository _jogadorQueryRepository;
    private readonly IJogadorCommandRepository _jogadorCommandRepository;

    public JogadorService(
        IJogadorQueryRepository jogadorQueryRepository,
        IJogadorCommandRepository jogadorCommandRepository)
    {
        _jogadorQueryRepository = jogadorQueryRepository;
        _jogadorCommandRepository = jogadorCommandRepository;
    }

    public async Task<IEnumerable<JogadorViewModel>> ObterDadosJogadores()
    {
        var jogadores = await _jogadorQueryRepository.ObterDadosJogadores();
        return jogadores.Select(MapearParaViewModel).ToList();
    }

    public async Task<JogadorViewModel?> ObterDadosJogador(int id)
    {
        var jogador = await _jogadorQueryRepository.ObterDadosJogador(id);
        return jogador is null ? null : MapearParaViewModel(jogador);
    }

    private static JogadorViewModel MapearParaViewModel(Jogador jogador)
    {
        var valorRestante = Math.Max(0m, jogador.TotalPagar - jogador.TotalPago);

        return new JogadorViewModel
        {
            Id = jogador.Id,
            UrlImagem = jogador.UrlImagem,
            Nome = jogador.Nome,
            Sobrenome = jogador.Sobrenome,
            TotalPago = jogador.TotalPago,
            TotalPagar = jogador.TotalPagar,
            UrlImagemComprovante = jogador.UrlImagemComprovante,
            PagouValorTotal = jogador.TotalPago >= jogador.TotalPagar,
            ValorRestante = valorRestante
        };
    }
}
