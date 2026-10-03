using Syner.Api.Data;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Services.Notificaciones;

public sealed class NotificacionService
{
    private readonly SynerDbContext _db;

    public NotificacionService(SynerDbContext db)
    {
        _db = db;
    }

    public async Task CrearAsync(
        long usuarioId,
        string tipo,
        string titulo,
        string mensaje,
        string? entidadTipo = null,
        long? entidadId = null,
        CancellationToken cancellationToken = default)
    {
        Crear(
            usuarioId,
            tipo,
            titulo,
            mensaje,
            entidadTipo,
            entidadId
        );

        await _db.SaveChangesAsync(cancellationToken);
    }

    public Notificacion Crear(
        long usuarioId,
        string tipo,
        string titulo,
        string mensaje,
        string? entidadTipo = null,
        long? entidadId = null)
    {
        var notificacion = new Notificacion
        {
            UsuarioId = usuarioId,
            Tipo = tipo,
            Titulo = titulo,
            Mensaje = mensaje,
            Leida = false,
            FechaCreacion = DateTime.UtcNow,
            EntidadTipo = entidadTipo,
            EntidadId = entidadId
        };

        _db.Notificaciones.Add(notificacion);

        return notificacion;
    }
}