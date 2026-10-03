using System.ComponentModel.DataAnnotations;
using Syner.Api.Data.Validation;

namespace Syner.Api.Domain.DTOs.Pedidos;

public sealed class CrearPedidoCupoRequest
{
    [Required]
    public long LoteId { get; init; }

    public ResultadoValidacion Validar()
    {
        var errores = new List<string>();

        if (LoteId <= 0)
        {
            errores.Add("El lote indicado no es válido.");
        }

        return new ResultadoValidacion
        {
            EsValido = errores.Count == 0,
            Errores = errores
        };
    }
}