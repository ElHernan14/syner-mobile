using System.ComponentModel.DataAnnotations;

using Syner.Api.Data.Validation;

namespace Syner.Api.Domain.DTOs.Auth;

public sealed class RefreshTokenRequest : IValidable
{
    [Required(ErrorMessage = "El refresh token es obligatorio.")]
    public string RefreshToken { get; set; } = string.Empty;

    public ResultadoValidacion Validar()
    {
        return ValidadorDto.Validar(this);
    }
}