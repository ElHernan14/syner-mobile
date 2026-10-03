using FirebaseAdmin.Messaging;

namespace Syner.Api.Firebase;

public sealed class FirebasePushService
{
    private readonly FirebaseMessaging _messaging;

    public FirebasePushService()
    {
        _messaging = FirebaseMessaging.DefaultInstance;
    }

    public async Task<string> EnviarAsync(
        string token,
        string titulo,
        string mensaje,
        Dictionary<string, string>? datos = null,
        CancellationToken cancellationToken = default)
    {
        var message = new Message
        {
            Token = token,

            Notification = new Notification
            {
                Title = titulo,
                Body = mensaje
            },

            Data = datos
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

        var mensajes = tokensLista
            .Select(token => new Message
            {
                Token = token,

                Notification = new Notification
                {
                    Title = titulo,
                    Body = mensaje
                },

                Data = datos
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
            catch
            {
                // Un token inválido no debe impedir
                // el envío al resto de los dispositivos.
            }
        }

        return resultados;
    }
}