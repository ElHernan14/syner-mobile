using System.ComponentModel.DataAnnotations;

namespace Syner.Api.Domain.DTOs.Lotes;

public sealed class CrearLoteRequest
{
    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Categoria { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal PrecioMercado { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal PrecioCupo { get; set; }

    [Range(0, 100)]
    public decimal PorcentajeAhorro { get; set; }

    [Range(1, int.MaxValue)]
    public int CantidadCupos { get; set; }

    [Required]
    public DateTime FechaInicio { get; set; }

    [Required]
    public DateTime FechaFin { get; set; }

    [Required]
    public int ProveedorId { get; set; }
}