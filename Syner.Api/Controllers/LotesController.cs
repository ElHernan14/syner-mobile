using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syner.Api.Data;

namespace Syner.Api.Controllers;

[ApiController]
[Route("api/lotes")]
public sealed class LotesController : ControllerBase
{
    private readonly SynerDbContext _db;

    public LotesController(SynerDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(CancellationToken cancellationToken)
    {
        var lotes = await _db.Lotes
            .AsNoTracking()
            .Select(lote => new
            {
                id = lote.Id,
                nombre = lote.Nombre,
                descripcion = lote.Descripcion,
                categoria = lote.Categoria,

                precio_mercado = (double)lote.PrecioMercado,
                precio_cupo = (double)lote.PrecioCupo,
                porcentaje_ahorro = (double)lote.PorcentajeAhorro,

                cantidad_cupos = lote.CantidadCupos,
                cupos_ocupados = lote.CuposOcupados,

                fecha_inicio = lote.FechaInicio,
                fecha_fin = lote.FechaFin,

                estado = lote.Estado,

                proveedor = new
                {
                    id = lote.Proveedor.Id,
                    nombre = lote.Proveedor.Nombre,
                    descripcion = lote.Proveedor.Descripcion,
                    verificado = lote.Proveedor.Verificado
                }
            })
            .ToListAsync(cancellationToken);

        return Ok(lotes);
    }
}