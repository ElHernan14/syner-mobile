using System.ComponentModel.DataAnnotations;
using Syner.Api.Data.Validation;


namespace Syner.Api.Domain.DTOs.Dispositivos;

public sealed class RegistrarDispositivoRequest
{
    [Required]
    public string Token { get; init; } = string.Empty;

    [Required]
    public string Plataforma { get; init; } = string.Empty;

    public ResultadoValidacion Validar()
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(Token))
        {
            errores.Add("El token del dispositivo es obligatorio.");
        }

        if (Token.Length > 500)
        {
            errores.Add(
                "El token del dispositivo no puede superar los 500 caracteres."
            );
        }

        if (string.IsNullOrWhiteSpace(Plataforma))
        {
            errores.Add("La plataforma del dispositivo es obligatoria.");
        }

        if (!string.IsNullOrWhiteSpace(Plataforma) &&
            Plataforma.Length > 30)
        {
            errores.Add(
                "La plataforma no puede superar los 30 caracteres."
            );
        }

        return new ResultadoValidacion
        {
            EsValido = errores.Count == 0,
            Errores = errores
        };
    }
}