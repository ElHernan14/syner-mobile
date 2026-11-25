using System.ComponentModel.DataAnnotations;
using Syner.Api.Data.Validation;

namespace Syner.Api.Domain.DTOs.Direcciones;

public sealed class GuardarDireccionRequest : IValidable
{
    [Required, MaxLength(150)]
    public string Calle { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Numero { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Ciudad { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Provincia { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string CodigoPostal { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Referencia { get; set; }

    [Range(typeof(decimal), "-90", "90")]
    public decimal Latitud { get; set; }

    [Range(typeof(decimal), "-180", "180")]
    public decimal Longitud { get; set; }

    public ResultadoValidacion Validar()
    {
        return ValidadorDto.Validar(this);
    }
}
