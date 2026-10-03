namespace Syner.Api.Domain.Entities;

public sealed class Notificacion : EntityBase
{
    public long UsuarioId { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;

    public bool Leida { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string? EntidadTipo { get; set; }

    public long? EntidadId { get; set; }

    public Usuario Usuario { get; set; } = null!;
}