using gerenciador_pagamentos.webapi.ViewModel;

namespace gerenciador_pagamentos.webapi.Interface;

public interface IJogadorService
{
    Task<IEnumerable<JogadorViewModel>> ObterDadosJogadores();
    Task<JogadorViewModel?> ObterDadosJogador(int id);
}
