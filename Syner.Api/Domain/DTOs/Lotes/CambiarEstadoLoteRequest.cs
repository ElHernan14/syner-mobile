using System.ComponentModel.DataAnnotations;
using Syner.Api.Data.Validation;

namespace Syner.Api.Domain.DTOs.Lotes;

public sealed class CambiarEstadoLoteRequest
{
    [Required]
    public string Estado { get; init; } = string.Empty;

    public ResultadoValidacion Validar()
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(Estado))
        {
            errores.Add("El estado es obligatorio.");
        }

        return new ResultadoValidacion
        {
            EsValido = errores.Count == 0,
            Errores = errores
        };
    }
}