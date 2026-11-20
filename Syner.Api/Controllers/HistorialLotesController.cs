using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syner.Api.Data;
using Syner.Api.Data.Responses;

namespace Syner.Api.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/historial/lotes")]
public sealed class HistorialLotesController(SynerDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int page = 1, [FromQuery] int limit = 20, CancellationToken cancellationToken = default)
    {
        if (page < 1 || limit is < 1 or > 100)
            return RespuestaHttp.BadRequest<object>(this, "La paginación no es válida.");

        var consulta = db.HistorialLotes.AsNoTracking().OrderByDescending(x => x.FechaModificacion).ThenByDescending(x => x.Id);
        var total = await consulta.CountAsync(cancellationToken);
        var filas = await consulta.Skip((page - 1) * limit).Take(limit).Select(x => new
        {
            id = x.Id,
            lote_id = x.LoteId,
            lote = x.Lote.Nombre,
            usuario_id = x.UsuarioId,
            usuario = x.Usuario.Nombre,
            estado_anterior = x.EstadoAnterior,
            nuevo_estado = x.NuevoEstado,
            fecha_modificacion = x.FechaModificacion
        }).ToListAsync(cancellationToken);

        var datos = filas.Select(fila => new
        {
            fila.id,
            fila.lote_id,
            fila.lote,
            fila.usuario_id,
            fila.usuario,
            fila.estado_anterior,
            fila.nuevo_estado,
            fecha_modificacion = DateTime.SpecifyKind(
                fila.fecha_modificacion,
                DateTimeKind.Utc
            )
        }).ToList();

        return RespuestaHttp.Ok(this, "Historial de lotes obtenido correctamente.", new
        {
            datos,
            page,
            limit,
            total,
            totalPaginas = (int)Math.Ceiling(total / (double)limit)
        });
    }
}
