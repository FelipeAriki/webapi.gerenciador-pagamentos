using gerenciador_pagamentos.webapi.Model;

namespace gerenciador_pagamentos.webapi.Interface;

public interface IJogadorCommandRepository
{
    Task<Jogador> CriarJogador(Jogador jogador, CancellationToken cancellationToken = default);
    Task<Jogador?> AlterarJogador(Jogador jogador, CancellationToken cancellationToken = default);
    Task<bool> ExcluirJogador(int id, CancellationToken cancellationToken = default);
}
