using System.ComponentModel.DataAnnotations;

namespace Syner.Api.Domain.DTOs.Auth;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;

    public (bool EsValido, List<string> Errores) Validar()
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(Correo))
        {
            errores.Add("El correo es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            errores.Add("La contraseña es obligatoria.");
        }

        return (
            errores.Count == 0,
            errores
        );
    }
}