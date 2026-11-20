using System.ComponentModel.DataAnnotations;
using Syner.Api.Data.Validation;

namespace Syner.Api.Domain.DTOs.Pedidos;

public sealed class CrearPedidoCupoRequest
{
    [Required]
    public long LoteId { get; init; }

    [Required]
    public IFormFile? Firma { get; init; }

    public ResultadoValidacion Validar()
    {
        var errores = new List<string>();

        if (LoteId <= 0)
        {
            errores.Add("El lote indicado no es válido.");
        }

        if (Firma is null || Firma.Length == 0)
        {
            errores.Add("La firma de conformidad es obligatoria.");
        }
        else if (Firma.Length > 1024 * 1024)
        {
            errores.Add("La firma supera el tamaño máximo de 1 MB.");
        }
        else if (!string.Equals(Firma.ContentType, "image/png", StringComparison.OrdinalIgnoreCase) ||
                 !Path.GetExtension(Firma.FileName).Equals(".png", StringComparison.OrdinalIgnoreCase))
        {
            errores.Add("La firma debe enviarse como una imagen PNG.");
        }

        return new ResultadoValidacion
        {
            EsValido = errores.Count == 0,
            Errores = errores
        };
    }
}
