namespace Syner.Api.Domain.Entities;

public sealed class HistorialLote : EntityBase
{
    public long LoteId { get; set; }
    public long UsuarioId { get; set; }
    public string EstadoAnterior { get; set; } = string.Empty;
    public string NuevoEstado { get; set; } = string.Empty;
    public DateTime FechaModificacion { get; set; }

    public Lote Lote { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
