using FirebaseAdmin;
using FirebaseAdmin.Messaging;

namespace Syner.Api.Firebase;

public sealed class FirebasePushService
{
    private readonly FirebaseMessaging? _messaging;
    private readonly ILogger<FirebasePushService> _logger;

    public FirebasePushService(
        FirebaseApp? firebaseApp,
        ILogger<FirebasePushService> logger)
    {
        _messaging = firebaseApp is null
            ? null
            : FirebaseMessaging.GetMessaging(firebaseApp);
        _logger = logger;
    }

    public async Task<string> EnviarAsync(
        string token,
        string titulo,
        string mensaje,
        Dictionary<string, string>? datos = null,
        CancellationToken cancellationToken = default)
    {
        if (_messaging is null)
        {
            _logger.LogWarning(
                "No se envió la push porque Firebase no está configurado en este entorno."
            );
            return string.Empty;
        }

        var message = new Message
        {
            Token = token,

            Notification = new Notification
            {
                Title = titulo,
                Body = mensaje
            },

            Data = datos,
            Android = CrearConfiguracionAndroid()
        };

        return await _messaging.SendAsync(
            message,
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<string>> EnviarMuchosAsync(
        IEnumerable<string> tokens,
        string titulo,
        string mensaje,
        Dictionary<string, string>? datos = null,
        CancellationToken cancellationToken = default)
    {
        var tokensLista = tokens
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .Distinct()
            .ToList();

        if (tokensLista.Count == 0)
        {
            return [];
        }

        if (_messaging is null)
        {
            _logger.LogWarning(
                "No se enviaron las push porque Firebase no está configurado en este entorno."
            );
            return [];
        }

        var mensajes = tokensLista
            .Select(token => new Message
            {
                Token = token,

                Notification = new Notification
                {
                    Title = titulo,
                    Body = mensaje
                },

                Data = datos,
                Android = CrearConfiguracionAndroid()
            })
            .ToList();

        var resultados = new List<string>();

        foreach (var message in mensajes)
        {
            try
            {
                var response = await _messaging.SendAsync(
                    message,
                    cancellationToken
                );

                resultados.Add(response);
            }
            catch (Exception ex)
            {
                // Un token inválido no debe impedir
                // el envío al resto de los dispositivos.
                _logger.LogWarning(
                    ex,
                    "Firebase rechazó una push para un dispositivo registrado. El token se omite en el log."
                );
            }
        }

        _logger.LogInformation(
            "Firebase aceptó {CantidadEnviada} de {CantidadObjetivo} notificaciones push.",
            resultados.Count,
            tokensLista.Count
        );

        return resultados;
    }

    private static AndroidConfig CrearConfiguracionAndroid()
    {
        return new AndroidConfig
        {
            Priority = Priority.High,
            Notification = new AndroidNotification
            {
                ChannelId = "syner_high_importance",
                Icon = "ic_stat_notificacion",
                Sound = "default"
            }
        };
    }
}
