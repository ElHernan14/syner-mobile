namespace Syner.Api.Domain.DTOs.Notificaciones;

public sealed class NotificacionResponse
{
    public long Id { get; init; }

    public string Tipo { get; init; } = string.Empty;

    public string Titulo { get; init; } = string.Empty;

    public string Mensaje { get; init; } = string.Empty;

    public bool Leida { get; init; }

    public DateTime FechaCreacion { get; init; }

    public string? EntidadTipo { get; init; }

    public long? EntidadId { get; init; }
}