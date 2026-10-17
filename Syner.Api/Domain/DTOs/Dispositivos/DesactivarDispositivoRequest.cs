using System.ComponentModel.DataAnnotations;
using Syner.Api.Data.Validation;

namespace Syner.Api.Domain.DTOs.Dispositivos;

public sealed class DesactivarDispositivoRequest
{
    [Required]
    public string Token { get; init; } = string.Empty;

    public ResultadoValidacion Validar()
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(Token))
        {
            errores.Add("El token del dispositivo es obligatorio.");
        }

        if (Token.Length > 500)
        {
            errores.Add("El token del dispositivo no puede superar los 500 caracteres.");
        }

        return new ResultadoValidacion
        {
            EsValido = errores.Count == 0,
            Errores = errores
        };
    }
}
