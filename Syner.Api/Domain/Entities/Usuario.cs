namespace Syner.Api.Domain.Entities;

public sealed class Usuario : EntityBase
{
    public string Nombre { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string Dni { get; set; } = string.Empty;

    public string Rol { get; set; } = "usuario";

    public string Estado { get; set; } = "pendiente";
}