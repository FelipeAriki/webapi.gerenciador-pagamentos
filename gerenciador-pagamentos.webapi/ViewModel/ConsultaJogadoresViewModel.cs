using System.ComponentModel.DataAnnotations;

namespace gerenciador_pagamentos.webapi.ViewModel;

public class ConsultaJogadoresViewModel
{
    [Range(1, 1000000, ErrorMessage = "A página deve estar entre 1 e 1.000.000.")]
    public int Pagina { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; set; } = 10;

    [StringLength(200, ErrorMessage = "A busca deve ter até 200 caracteres.")]
    public string? Busca { get; set; }

    [Required]
    [RegularExpression("^(todos|quitado|pendente)$", ErrorMessage = "A situação deve ser todos, quitado ou pendente.")]
    public string Situacao { get; set; } = "todos";
}
