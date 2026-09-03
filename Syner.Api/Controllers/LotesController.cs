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
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] Paginacion paginacion,
        CancellationToken cancellationToken)
    {
        try
        {
            paginacion.Validar();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }

        var consulta = _db.Lotes
            .AsNoTracking()
            .AsQueryable();

        // Filtro por término
        if (!string.IsNullOrWhiteSpace(paginacion.Termino))
        {
            var termino = paginacion.Termino.Trim();

            consulta = consulta.Where(lote =>
                lote.Nombre.Contains(termino) ||
                lote.Descripcion.Contains(termino) ||
                lote.Categoria.Contains(termino));
        }

        // Ordenamiento
        consulta = paginacion.Sort?.ToLower() switch
        {
            "nombre" => consulta.OrderBy(lote => lote.Nombre),
            "nombre_desc" => consulta.OrderByDescending(lote => lote.Nombre),

            "precio" => consulta.OrderBy(lote => lote.PrecioCupo),
            "precio_desc" => consulta.OrderByDescending(lote => lote.PrecioCupo),

            "fecha_inicio" => consulta.OrderBy(lote => lote.FechaInicio),
            "fecha_inicio_desc" => consulta.OrderByDescending(lote => lote.FechaInicio),

            "estado" => consulta.OrderBy(lote => lote.Estado),
            "estado_desc" => consulta.OrderByDescending(lote => lote.Estado),

            _ => consulta.OrderBy(lote => lote.Id)
        };

        // Total antes de aplicar Skip/Take
        var total = await consulta.CountAsync(cancellationToken);

        // Offset calculado por el servidor
        var offset = paginacion.CalcularOffset();

        var lotes = await consulta
            .Skip(offset)
            .Take(paginacion.Limit)
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

        var resultado = new ResultadoPaginado<object>
        {
            Datos = lotes,
            Page = paginacion.Page,
            Limit = paginacion.Limit,
            Offset = offset,
            Total = total
        };

        return Ok(resultado);
    }
}