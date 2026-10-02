using gerenciador_pagamentos.webapi.ViewModel;

namespace gerenciador_pagamentos.webapi.Interface;

public interface IJogadorService
{
    Task<PaginaJogadoresViewModel> ObterDadosJogadores(ConsultaJogadoresViewModel consulta, CancellationToken cancellationToken = default);
    Task<JogadorViewModel?> ObterDadosJogador(int id, CancellationToken cancellationToken = default);
    Task<JogadorViewModel> CriarJogador(SalvarJogadorViewModel dados, CancellationToken cancellationToken = default);
    Task<JogadorViewModel?> AlterarJogador(int id, SalvarJogadorViewModel dados, CancellationToken cancellationToken = default);
    Task<bool> ExcluirJogador(int id, CancellationToken cancellationToken = default);
}
