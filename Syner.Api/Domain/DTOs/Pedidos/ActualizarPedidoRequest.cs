using System.ComponentModel.DataAnnotations;
using Syner.Api.Data.Validation;

namespace Syner.Api.Domain.DTOs.Pedidos;

public sealed class ActualizarPedidoRequest : IValidable
{
    [Range(1, long.MaxValue, ErrorMessage = "El usuario es obligatorio.")]
    public long UsuarioId { get; set; }

    [Range(1, long.MaxValue, ErrorMessage = "El lote es obligatorio.")]
    public long LoteId { get; set; }

    [MaxLength(100, ErrorMessage = "El número de seguimiento no puede superar los 100 caracteres.")]
    public string? NumeroSeguimiento { get; set; }

    [MaxLength(50, ErrorMessage = "El código de entrega no puede superar los 50 caracteres.")]
    public string? CodigoEntrega { get; set; }

    public ResultadoValidacion Validar()
    {
        return ValidadorDto.Validar(this);
    }
}