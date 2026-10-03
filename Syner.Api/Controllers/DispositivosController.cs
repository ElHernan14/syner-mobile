using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syner.Api.Data;
using Syner.Api.Data.Responses;
using Syner.Api.Domain.DTOs.Dispositivos;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Controllers;

[ApiController]
[Route("api/dispositivos")]
[Authorize]
public sealed class DispositivosController : ControllerBase
{
    private readonly SynerDbContext _db;

    public DispositivosController(SynerDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Registrar(
        RegistrarDispositivoRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return RespuestaHttp.BadRequest<DispositivoResponse>(
                this,
                "La solicitud contiene datos inválidos.",
                ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray()
            );
        }

        var validacion = request.Validar();

        if (!validacion.EsValido)
        {
            return RespuestaHttp.BadRequest<DispositivoResponse>(
                this,
                "No se pudo registrar el dispositivo.",
                validacion.Errores.ToArray()
            );
        }

        var usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
        {
            return RespuestaHttp.BadRequest<DispositivoResponse>(
                this,
                "No se pudo identificar al usuario autenticado."
            );
        }

        var dispositivo = await _db.Dispositivos
            .FirstOrDefaultAsync(
                x => x.Token == request.Token,
                cancellationToken
            );

        if (dispositivo is null)
        {
            dispositivo = new Dispositivo
            {
                UsuarioId = usuarioId.Value,
                Token = request.Token.Trim(),
                Plataforma = request.Plataforma.Trim().ToLowerInvariant(),
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            };

            _db.Dispositivos.Add(dispositivo);
        }
        else
        {
            dispositivo.UsuarioId = usuarioId.Value;
            dispositivo.Plataforma =
                request.Plataforma.Trim().ToLowerInvariant();
            dispositivo.Activo = true;
            dispositivo.FechaRegistro = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        var respuesta = new DispositivoResponse
        {
            Id = dispositivo.Id,
            Plataforma = dispositivo.Plataforma,
            Activo = dispositivo.Activo,
            FechaRegistro = dispositivo.FechaRegistro
        };

        return RespuestaHttp.Ok(
            this,
            "Dispositivo registrado correctamente.",
            respuesta
        );
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerMisDispositivos(
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
        {
            return RespuestaHttp.BadRequest<List<DispositivoResponse>>(
                this,
                "No se pudo identificar al usuario autenticado."
            );
        }

        var dispositivos = await _db.Dispositivos
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId.Value)
            .OrderByDescending(x => x.FechaRegistro)
            .Select(x => new DispositivoResponse
            {
                Id = x.Id,
                Plataforma = x.Plataforma,
                Activo = x.Activo,
                FechaRegistro = x.FechaRegistro
            })
            .ToListAsync(cancellationToken);

        return RespuestaHttp.Ok(
            this,
            "Dispositivos obtenidos correctamente.",
            dispositivos
        );
    }

    [HttpPut("{id}/desactivar")]
    public async Task<IActionResult> Desactivar(
        long id,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
        {
            return RespuestaHttp.BadRequest<DispositivoResponse>(
                this,
                "No se pudo identificar al usuario autenticado."
            );
        }

        var dispositivo = await _db.Dispositivos
            .FirstOrDefaultAsync(
                x =>
                    x.Id == id &&
                    x.UsuarioId == usuarioId.Value,
                cancellationToken
            );

        if (dispositivo is null)
        {
            return RespuestaHttp.NotFound<DispositivoResponse>(
                this,
                "Dispositivo no encontrado."
            );
        }

        dispositivo.Activo = false;

        await _db.SaveChangesAsync(cancellationToken);

        var respuesta = new DispositivoResponse
        {
            Id = dispositivo.Id,
            Plataforma = dispositivo.Plataforma,
            Activo = dispositivo.Activo,
            FechaRegistro = dispositivo.FechaRegistro
        };

        return RespuestaHttp.Ok(
            this,
            "Dispositivo desactivado correctamente.",
            respuesta
        );
    }

    private long? ObtenerUsuarioId()
    {
        var claim =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return long.TryParse(claim, out var usuarioId)
            ? usuarioId
            : null;
    }
}