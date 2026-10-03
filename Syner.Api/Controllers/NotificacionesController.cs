using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syner.Api.Data;
using Syner.Api.Data.Responses;
using Syner.Api.Domain.DTOs.Notificaciones;

namespace Syner.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/notificaciones")]
public sealed class NotificacionesController : ControllerBase
{
    private readonly SynerDbContext _db;

    public NotificacionesController(SynerDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas(
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "No se pudo identificar al usuario autenticado."
            );
        }

        var notificaciones = await _db.Notificaciones
            .AsNoTracking()
            .Where(notificacion =>
                notificacion.UsuarioId == usuarioId.Value)
            .OrderByDescending(notificacion =>
                notificacion.FechaCreacion)
            .Select(notificacion => new NotificacionResponse
            {
                Id = notificacion.Id,
                Tipo = notificacion.Tipo,
                Titulo = notificacion.Titulo,
                Mensaje = notificacion.Mensaje,
                Leida = notificacion.Leida,
                FechaCreacion = notificacion.FechaCreacion,
                EntidadTipo = notificacion.EntidadTipo,
                EntidadId = notificacion.EntidadId
            })
            .ToListAsync(cancellationToken);

        return RespuestaHttp.Ok(
            this,
            "Notificaciones obtenidas correctamente.",
            notificaciones
        );
    }

    [HttpGet("no-leidas")]
    public async Task<IActionResult> ObtenerNoLeidas(
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "No se pudo identificar al usuario autenticado."
            );
        }

        var notificaciones = await _db.Notificaciones
            .AsNoTracking()
            .Where(notificacion =>
                notificacion.UsuarioId == usuarioId.Value &&
                !notificacion.Leida)
            .OrderByDescending(notificacion =>
                notificacion.FechaCreacion)
            .Select(notificacion => new NotificacionResponse
            {
                Id = notificacion.Id,
                Tipo = notificacion.Tipo,
                Titulo = notificacion.Titulo,
                Mensaje = notificacion.Mensaje,
                Leida = notificacion.Leida,
                FechaCreacion = notificacion.FechaCreacion,
                EntidadTipo = notificacion.EntidadTipo,
                EntidadId = notificacion.EntidadId
            })
            .ToListAsync(cancellationToken);

        return RespuestaHttp.Ok(
            this,
            "Notificaciones no leídas obtenidas correctamente.",
            notificaciones
        );
    }

    [HttpPut("{id:long}/leer")]
    public async Task<IActionResult> MarcarComoLeida(
        long id,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "No se pudo identificar al usuario autenticado."
            );
        }

        var notificacion = await _db.Notificaciones
            .FirstOrDefaultAsync(
                item =>
                    item.Id == id &&
                    item.UsuarioId == usuarioId.Value,
                cancellationToken
            );

        if (notificacion is null)
        {
            return RespuestaHttp.NotFound<object>(
                this,
                "La notificación no existe."
            );
        }

        if (!notificacion.Leida)
        {
            notificacion.Leida = true;

            await _db.SaveChangesAsync(cancellationToken);
        }

        return RespuestaHttp.Ok(
            this,
            "Notificación marcada como leída.",
            new
            {
                id = notificacion.Id,
                leida = notificacion.Leida
            }
        );
    }

    private long? ObtenerUsuarioId()
    {
        var valor = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return long.TryParse(valor, out var usuarioId)
            ? usuarioId
            : null;
    }
}