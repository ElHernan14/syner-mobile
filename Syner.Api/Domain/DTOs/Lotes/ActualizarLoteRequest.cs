using System.ComponentModel.DataAnnotations;

namespace Syner.Api.Domain.DTOs.Lotes;

public sealed class ActualizarLoteRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [MaxLength(50, ErrorMessage = "La categoría no puede superar los 50 caracteres.")]
    public string Categoria { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio de mercado debe ser mayor a 0.")]
    public decimal PrecioMercado { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio de cupo debe ser mayor a 0.")]
    public decimal PrecioCupo { get; set; }

    [Range(0, 100, ErrorMessage = "El porcentaje de ahorro debe estar entre 0 y 100.")]
    public decimal PorcentajeAhorro { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad de cupos debe ser al menos 1.")]
    public int CantidadCupos { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTime FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
    public DateTime FechaFin { get; set; }

    [Required(ErrorMessage = "El proveedor es obligatorio.")]
    public int ProveedorId { get; set; }
}