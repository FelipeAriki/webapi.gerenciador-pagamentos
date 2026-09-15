namespace gerenciador_pagamentos.webapi.ViewModel;

public class JogadorViewModel
{
    public int Id { get; set; }
    public string? UrlImagem { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Sobrenome { get; set; } = string.Empty;
    public decimal TotalPago { get; set; }
    public decimal TotalPagar { get; set; }
    public string? UrlImagemComprovante { get; set; }
    public bool PagouValorTotal { get; set; }
    public decimal ValorRestante { get; set; }
}
