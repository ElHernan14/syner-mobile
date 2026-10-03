namespace Syner.Api.Domain.Entities;

public sealed class Dispositivo : EntityBase
{
    public long UsuarioId { get; set; }

    public string Token { get; set; } = string.Empty;

    public string Plataforma { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public Usuario Usuario { get; set; } = null!;
}