using gerenciador_pagamentos.webapi.Interface;
using gerenciador_pagamentos.webapi.Model;
using gerenciador_pagamentos.webapi.ViewModel;

namespace gerenciador_pagamentos.webapi.Tests;

public class MemoryJogadorRepository : IJogadorQueryRepository, IJogadorCommandRepository
{
    private readonly Dictionary<int, Jogador> jogadores = [];
    private int proximoId;
    public CancellationToken UltimoToken { get; private set; }
    public bool Falhar { get; set; }

    private void Verificar(CancellationToken token)
    {
        UltimoToken = token;
        token.ThrowIfCancellationRequested();
        if (Falhar) throw new InvalidOperationException("Mensagem interna confidencial de teste");
    }

    public Task<Jogador> CriarJogador(Jogador jogador, CancellationToken cancellationToken = default)
    {
        Verificar(cancellationToken);
        jogador.Id = ++proximoId;
        jogadores.Add(jogador.Id, jogador);
        return Task.FromResult(jogador);
    }
    public Task<Jogador?> AlterarJogador(Jogador jogador, CancellationToken cancellationToken = default)
    {
        Verificar(cancellationToken);
        if (!jogadores.ContainsKey(jogador.Id)) return Task.FromResult<Jogador?>(null);
        jogadores[jogador.Id] = jogador;
        return Task.FromResult<Jogador?>(jogador);
    }
    public Task<bool> ExcluirJogador(int id, CancellationToken cancellationToken = default)
    {
        Verificar(cancellationToken);
        return Task.FromResult(jogadores.Remove(id));
    }
    public Task<Jogador?> ObterDadosJogador(int id, CancellationToken cancellationToken = default)
    {
        Verificar(cancellationToken);
        return Task.FromResult(jogadores.GetValueOrDefault(id));
    }
    public Task<(IReadOnlyList<Jogador> Itens, ResumoPagamentosViewModel Resumo)> ObterDadosJogadores(ConsultaJogadoresViewModel consulta, CancellationToken cancellationToken = default)
    {
        Verificar(cancellationToken);
        var lista = jogadores.Values.Where(j =>
            (string.IsNullOrWhiteSpace(consulta.Busca) || (j.Nome + " " + j.Sobrenome).Contains(consulta.Busca.Trim(), StringComparison.OrdinalIgnoreCase)) &&
            (consulta.Situacao == "todos" || (consulta.Situacao == "quitado" ? j.TotalPago >= j.TotalPagar : j.TotalPago < j.TotalPagar)))
            .OrderBy(j => j.Nome).ThenBy(j => j.Sobrenome).ThenBy(j => j.Id).ToList();
        var resumo = new ResumoPagamentosViewModel
        {
            TotalJogadores = lista.Count,
            Quitados = lista.Count(j => j.TotalPago >= j.TotalPagar),
            Pendentes = lista.Count(j => j.TotalPago < j.TotalPagar),
            TotalPago = lista.Sum(j => j.TotalPago),
            TotalPagar = lista.Sum(j => j.TotalPagar),
            ValorRestante = lista.Sum(j => Math.Max(0m, j.TotalPagar - j.TotalPago))
        };
        IReadOnlyList<Jogador> pagina = lista.Skip((consulta.Pagina - 1) * consulta.TamanhoPagina).Take(consulta.TamanhoPagina).ToList();
        return Task.FromResult((pagina, resumo));
    }
}
