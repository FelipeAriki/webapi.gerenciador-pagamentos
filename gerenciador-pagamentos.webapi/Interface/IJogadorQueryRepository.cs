using gerenciador_pagamentos.webapi.Model;
using gerenciador_pagamentos.webapi.ViewModel;

namespace gerenciador_pagamentos.webapi.Interface;

public interface IJogadorQueryRepository
{
    Task<(IReadOnlyList<Jogador> Itens, ResumoPagamentosViewModel Resumo)> ObterDadosJogadores(ConsultaJogadoresViewModel consulta, CancellationToken cancellationToken = default);
    Task<Jogador?> ObterDadosJogador(int id, CancellationToken cancellationToken = default);
}
