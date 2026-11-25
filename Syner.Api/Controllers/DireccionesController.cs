using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syner.Api.Data;
using Syner.Api.Data.Responses;
using Syner.Api.Data.Validation;
using Syner.Api.Domain.DTOs.Direcciones;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Controllers;

[ApiController]
[Route("api/direcciones")]
[Authorize]
public sealed class DireccionesController : ControllerBase
{
    private readonly SynerDbContext _db;

    public DireccionesController(SynerDbContext db)
    {
        _db = db;
    }

    [HttpGet("mi-direccion")]
    public async Task<IActionResult> ObtenerMiDireccion(CancellationToken cancellationToken)
    {
        var usuario_id = ObtenerUsuarioId();
        if (usuario_id is null)
            return RespuestaHttp.BadRequest<DireccionResponse?>(this, "No se pudo identificar al usuario autenticado.");

        var direccion = await _db.Direcciones
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuario_id.Value && x.EsPrincipal && x.FechaEliminacion == null)
            .OrderByDescending(x => x.FechaActualizacion)
            .Select(x => new DireccionResponse(
                x.Id,
                x.Calle,
                x.Numero,
                x.Ciudad,
                x.Provincia,
                x.CodigoPostal,
                x.Referencia,
                x.Latitud,
                x.Longitud,
                x.EsPrincipal,
                x.FechaCreacion,
                x.FechaActualizacion))
            .FirstOrDefaultAsync(cancellationToken);

        return RespuestaHttp.Ok(this, "Dirección principal obtenida correctamente.", direccion);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        GuardarDireccionRequest request,
        CancellationToken cancellationToken)
    {
        var validacion = Validar(request);
        if (validacion is not null) return validacion;

        var usuario_id = ObtenerUsuarioId();
        if (usuario_id is null)
            return RespuestaHttp.BadRequest<DireccionResponse>(this, "No se pudo identificar al usuario autenticado.");

        var existente = await _db.Direcciones.AnyAsync(
            x => x.UsuarioId == usuario_id.Value && x.EsPrincipal && x.FechaEliminacion == null,
            cancellationToken);
        if (existente)
            return RespuestaHttp.Conflict<DireccionResponse>(this, "Ya existe una dirección principal. Actualizala desde la aplicación.");

        var ahora = DateTime.UtcNow;
        var direccion = CrearEntidad(request, usuario_id.Value, ahora);
        _db.Direcciones.Add(direccion);
        await _db.SaveChangesAsync(cancellationToken);

        var respuesta = Mapear(direccion);
        return RespuestaHttp.Created(
            this,
            "Dirección guardada correctamente.",
            respuesta,
            nameof(ObtenerMiDireccion),
            new { });
    }

    [HttpPut("mi-direccion")]
    public async Task<IActionResult> Actualizar(
        GuardarDireccionRequest request,
        CancellationToken cancellationToken)
    {
        var validacion = Validar(request);
        if (validacion is not null) return validacion;

        var usuario_id = ObtenerUsuarioId();
        if (usuario_id is null)
            return RespuestaHttp.BadRequest<DireccionResponse>(this, "No se pudo identificar al usuario autenticado.");

        var direccion = await _db.Direcciones.FirstOrDefaultAsync(
            x => x.UsuarioId == usuario_id.Value && x.EsPrincipal && x.FechaEliminacion == null,
            cancellationToken);
        if (direccion is null)
            return RespuestaHttp.NotFound<DireccionResponse>(this, "Todavía no existe una dirección para actualizar.");

        ActualizarEntidad(direccion, request);
        direccion.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return RespuestaHttp.Ok(this, "Dirección actualizada correctamente.", Mapear(direccion));
    }

    private IActionResult? Validar(GuardarDireccionRequest request)
    {
        var validacion_model_state = ValidadorModelState.Validar(ModelState);
        if (!validacion_model_state.EsValido)
            return RespuestaHttp.BadRequest<object>(
                this,
                "La dirección contiene datos inválidos.",
                validacion_model_state.Errores.ToArray());

        var validacion = request.Validar();
        if (!validacion.EsValido)
            return RespuestaHttp.BadRequest<object>(
                this,
                "La dirección contiene datos inválidos.",
                validacion.Errores.ToArray());

        return null;
    }

    private long? ObtenerUsuarioId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return long.TryParse(claim, out var usuario_id) ? usuario_id : null;
    }

    private static Direccion CrearEntidad(
        GuardarDireccionRequest request,
        long usuario_id,
        DateTime ahora)
    {
        var direccion = new Direccion
        {
            UsuarioId = usuario_id,
            FechaCreacion = ahora,
            FechaActualizacion = ahora,
            EsPrincipal = true
        };
        ActualizarEntidad(direccion, request);
        return direccion;
    }

    private static void ActualizarEntidad(Direccion direccion, GuardarDireccionRequest request)
    {
        direccion.Calle = request.Calle.Trim();
        direccion.Numero = request.Numero.Trim();
        direccion.Ciudad = request.Ciudad.Trim();
        direccion.Provincia = request.Provincia.Trim();
        direccion.CodigoPostal = request.CodigoPostal.Trim();
        direccion.Referencia = string.IsNullOrWhiteSpace(request.Referencia)
            ? null
            : request.Referencia.Trim();
        direccion.Latitud = request.Latitud;
        direccion.Longitud = request.Longitud;
    }

    private static DireccionResponse Mapear(Direccion direccion) => new(
        direccion.Id,
        direccion.Calle,
        direccion.Numero,
        direccion.Ciudad,
        direccion.Provincia,
        direccion.CodigoPostal,
        direccion.Referencia,
        direccion.Latitud,
        direccion.Longitud,
        direccion.EsPrincipal,
        direccion.FechaCreacion,
        direccion.FechaActualizacion);
}
