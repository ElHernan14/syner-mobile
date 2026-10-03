namespace Syner.Api.Domain.DTOs.Dispositivos;

public sealed class DispositivoResponse
{
    public long Id { get; init; }
    public string Plataforma { get; init; } = string.Empty;
    public bool Activo { get; init; }
    public DateTime FechaRegistro { get; init; }
}