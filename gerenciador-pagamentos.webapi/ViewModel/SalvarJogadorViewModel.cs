using System.ComponentModel.DataAnnotations;

namespace gerenciador_pagamentos.webapi.ViewModel;

public class SalvarJogadorViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, ErrorMessage = "O nome deve ter até 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o sobrenome.")]
    [StringLength(100, ErrorMessage = "O sobrenome deve ter até 100 caracteres.")]
    public string Sobrenome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o total pago.")]
    [Range(typeof(decimal), "0", "99999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "O total pago deve estar entre 0 e 99.999.999,99.")]
    public decimal? TotalPago { get; set; }

    [Required(ErrorMessage = "Informe o total a pagar.")]
    [Range(typeof(decimal), "0", "99999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "O total a pagar deve estar entre 0 e 99.999.999,99.")]
    public decimal? TotalPagar { get; set; }

    [StringLength(500, ErrorMessage = "A URL da foto deve ter até 500 caracteres.")]
    public string? UrlImagem { get; set; }

    [StringLength(500, ErrorMessage = "A URL do comprovante deve ter até 500 caracteres.")]
    public string? UrlImagemComprovante { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var (valor, campo) in new[] { (TotalPago, nameof(TotalPago)), (TotalPagar, nameof(TotalPagar)) })
        {
            if (valor.HasValue && decimal.Round(valor.Value, 2) != valor.Value)
                yield return new ValidationResult("Informe um valor com no máximo duas casas decimais.", [campo]);
        }
        foreach (var (url, campo) in new[] { (UrlImagem, nameof(UrlImagem)), (UrlImagemComprovante, nameof(UrlImagemComprovante)) })
        {
            if (!string.IsNullOrWhiteSpace(url) &&
                (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) ||
                 (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
                yield return new ValidationResult("Informe uma URL válida começando com http:// ou https://.", [campo]);
        }
    }
}
