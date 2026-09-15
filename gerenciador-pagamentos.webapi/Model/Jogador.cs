namespace gerenciador_pagamentos.webapi.Model;

public class Jogador
{
    public int Id { get; set; }
    public string? UrlImagem { get; set; }
    public string? UrlImagemComprovante { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Sobrenome { get; set; } = string.Empty;
    public decimal TotalPago { get; set; }
    public decimal TotalPagar { get; set; }
}
