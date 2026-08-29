namespace Syner.Api.Domain.Entities;

public sealed class Lote : EntityBase
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;

    public decimal PrecioMercado { get; set; }
    public decimal PrecioCupo { get; set; }
    public decimal PorcentajeAhorro { get; set; }

    public int CantidadCupos { get; set; }
    public int CuposOcupados { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    public string Estado { get; set; } = string.Empty;

    public long ProveedorId { get; set; }

    public Proveedor Proveedor { get; set; } = null!;
}