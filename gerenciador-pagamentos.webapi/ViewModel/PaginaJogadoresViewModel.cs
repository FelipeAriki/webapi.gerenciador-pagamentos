namespace gerenciador_pagamentos.webapi.ViewModel;

public class ResumoPagamentosViewModel
{
    public long TotalJogadores { get; set; }
    public long Quitados { get; set; }
    public long Pendentes { get; set; }
    public decimal TotalPago { get; set; }
    public decimal TotalPagar { get; set; }
    public decimal ValorRestante { get; set; }
}

public class PaginaJogadoresViewModel
{
    public IReadOnlyList<JogadorViewModel> Itens { get; set; } = [];
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public long Total => Resumo.TotalJogadores;
    public long TotalPaginas => (long)Math.Ceiling((decimal)Total / TamanhoPagina);
    public ResumoPagamentosViewModel Resumo { get; set; } = new();
}
