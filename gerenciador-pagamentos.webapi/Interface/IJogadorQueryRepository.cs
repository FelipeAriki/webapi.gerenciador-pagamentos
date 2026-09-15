using gerenciador_pagamentos.webapi.Model;

namespace gerenciador_pagamentos.webapi.Interface;

public interface IJogadorQueryRepository
{
    Task<IEnumerable<Jogador>> ObterDadosJogadores();
    Task<Jogador?> ObterDadosJogador(int id);
}
