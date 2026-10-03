using Syner.Api.Firebase;

namespace Syner.Api.Services.Notificaciones;

public sealed class NotificacionPushService
{
    private readonly DispositivoService _dispositivo_service;
    private readonly FirebasePushService _firebase_push_service;
    private readonly ILogger<NotificacionPushService> _logger;

    public NotificacionPushService(
        DispositivoService dispositivo_service,
        FirebasePushService firebase_push_service,
        ILogger<NotificacionPushService> logger)
    {
        _dispositivo_service = dispositivo_service;
        _firebase_push_service = firebase_push_service;
        _logger = logger;
    }

    public async Task EnviarAUsuariosAsync(
        IEnumerable<long> usuarioIds,
        string titulo,
        string mensaje,
        string tipo,
        string entidadTipo,
        long entidadId,
        CancellationToken cancellationToken = default)
    {
        var ids = usuarioIds
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return;
        }

        var tokens =
            await _dispositivo_service.ObtenerTokensActivosAsync(
                ids,
                cancellationToken
            );

        if (tokens.Count == 0)
        {
            return;
        }

        var datos = new Dictionary<string, string>
        {
            ["tipo"] = tipo,
            ["entidadTipo"] = entidadTipo,
            ["entidadId"] = entidadId.ToString()
        };

        try
        {
            await _firebase_push_service.EnviarMuchosAsync(
                tokens,
                titulo,
                mensaje,
                datos,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No se pudieron enviar las notificaciones Push."
            );
        }
    }
}