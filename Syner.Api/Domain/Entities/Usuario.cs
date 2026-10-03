namespace Syner.Api.Domain.Entities;

public sealed class Usuario : EntityBase
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public long? RolId { get; set; }
    public Rol? Rol { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string Estado { get; set; } = "pendiente";

    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();

    public ICollection<Notificacion> Notificaciones { get; set; }
        = new List<Notificacion>();

    public ICollection<Dispositivo> Dispositivos { get; set; } = [];

    public string? Avatar { get; set; }
}