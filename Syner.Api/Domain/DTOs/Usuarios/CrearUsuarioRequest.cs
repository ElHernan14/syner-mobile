using System.ComponentModel.DataAnnotations;
using Syner.Api.Data.Validation;

namespace Syner.Api.Domain.DTOs.Usuarios;

public sealed class CrearUsuarioRequest : IValidable
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [MaxLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [MaxLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El DNI no puede superar los 20 caracteres.")]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El rol no puede superar los 50 caracteres.")]
    public string Rol { get; set; } = string.Empty;

    public ResultadoValidacion Validar()
    {
        return ValidadorDto.Validar(this);
    }
}