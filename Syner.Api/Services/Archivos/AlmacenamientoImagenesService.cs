using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace Syner.Api.Services.Archivos;

public sealed class AlmacenamientoImagenesService : IAlmacenamientoImagenesService
{
    private readonly HttpClient _httpClient;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AlmacenamientoImagenesService> _logger;
    private readonly bool _usarDiscoLocal;
    private readonly string? _supabaseUrl;
    private readonly string? _supabaseBucket;
    private readonly string? _supabaseServiceRoleKey;

    public AlmacenamientoImagenesService(
        HttpClient httpClient,
        IWebHostEnvironment environment,
        IOptions<SupabaseStorageOptions> storageOptions,
        ILogger<AlmacenamientoImagenesService> logger)
    {
        _httpClient = httpClient;
        _environment = environment;
        _logger = logger;
        _usarDiscoLocal = environment.IsDevelopment();

        var options = storageOptions.Value;
        if (options.IsConfigured)
        {
            var supabaseUri = new Uri(options.Url!);
            _supabaseUrl = supabaseUri.GetLeftPart(UriPartial.Authority);
            _supabaseBucket = options.Bucket!.Trim();
            _supabaseServiceRoleKey = options.ServiceRoleKey!.Trim();
        }
    }

    public async Task<string> SubirImagenLoteAsync(
        IFormFile imagen,
        CancellationToken cancellationToken)
    {
        var extension = ObtenerExtension(imagen);
        var nombre = $"{Guid.NewGuid():N}{extension}";

        if (_supabaseUrl is not null)
        {
            return await SubirASupabaseAsync(
                imagen,
                nombre,
                extension,
                cancellationToken
            );
        }

        if (!_usarDiscoLocal)
        {
            throw new AlmacenamientoImagenesException(
                "Supabase Storage no está configurado para este entorno."
            );
        }

        return await GuardarEnDiscoAsync(imagen, nombre, cancellationToken);
    }

    private async Task<string> SubirASupabaseAsync(
        IFormFile imagen,
        string nombre,
        string extension,
        CancellationToken cancellationToken)
    {
        var rutaObjeto = $"lotes/{nombre}";
        var bucket = Uri.EscapeDataString(_supabaseBucket!);
        var uri = new Uri(
            $"{_supabaseUrl}/storage/v1/object/{bucket}/{rutaObjeto}"
        );

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.TryAddWithoutValidation("apikey", _supabaseServiceRoleKey);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _supabaseServiceRoleKey
        );
        request.Headers.TryAddWithoutValidation("x-upsert", "false");
        request.Content = new StreamContent(imagen.OpenReadStream());
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(
            ObtenerTipoContenido(extension)
        );

        try
        {
            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Supabase Storage rechazó la carga de una imagen de lote con estado HTTP {StatusCode}.",
                    (int)response.StatusCode
                );

                throw new AlmacenamientoImagenesException(
                    "Supabase Storage no aceptó la imagen."
                );
            }
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "No se pudo conectar con Supabase Storage para subir una imagen de lote."
            );
            throw new AlmacenamientoImagenesException(
                "No se pudo conectar con Supabase Storage.",
                exception
            );
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "La carga de una imagen de lote a Supabase Storage excedió el tiempo de espera."
            );
            throw new AlmacenamientoImagenesException(
                "Supabase Storage no respondió a tiempo.",
                exception
            );
        }

        return $"{_supabaseUrl}/storage/v1/object/public/{bucket}/{rutaObjeto}";
    }

    private async Task<string> GuardarEnDiscoAsync(
        IFormFile imagen,
        string nombre,
        CancellationToken cancellationToken)
    {
        var webRoot = _environment.WebRootPath ??
            Path.Combine(_environment.ContentRootPath, "wwwroot");
        var directorio = Path.Combine(webRoot, "uploads", "lotes");
        var ruta = Path.Combine(directorio, nombre);

        try
        {
            Directory.CreateDirectory(directorio);
            await using var destino = new FileStream(
                ruta,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None
            );
            await using var origen = imagen.OpenReadStream();
            await origen.CopyToAsync(destino, cancellationToken);
        }
        catch (IOException exception)
        {
            EliminarArchivoParcial(ruta);
            _logger.LogError(
                exception,
                "No se pudo guardar en disco la imagen de un lote."
            );
            throw new AlmacenamientoImagenesException(
                "No se pudo guardar la imagen en el servidor.",
                exception
            );
        }
        catch (UnauthorizedAccessException exception)
        {
            EliminarArchivoParcial(ruta);
            _logger.LogError(
                exception,
                "No hay permisos para guardar en disco la imagen de un lote."
            );
            throw new AlmacenamientoImagenesException(
                "El servidor no tiene permisos para guardar la imagen.",
                exception
            );
        }

        return $"/uploads/lotes/{nombre}";
    }

    private void EliminarArchivoParcial(string ruta)
    {
        try
        {
            if (File.Exists(ruta))
            {
                File.Delete(ruta);
            }
        }
        catch (IOException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo limpiar un archivo parcial de imagen en {Ruta}.",
                ruta
            );
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo limpiar un archivo parcial de imagen en {Ruta}.",
                ruta
            );
        }
    }

    private static string ObtenerExtension(IFormFile imagen)
    {
        var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
        var tipo = imagen.ContentType.ToLowerInvariant();

        return (extension, tipo) switch
        {
            (".jpg" or ".jpeg", "image/jpeg") => extension,
            (".png", "image/png") => extension,
            (".webp", "image/webp") => extension,
            _ => throw new AlmacenamientoImagenesException(
                "La imagen debe ser JPG, PNG o WEBP."
            )
        };
    }

    private static string ObtenerTipoContenido(string extension) => extension switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => throw new AlmacenamientoImagenesException(
            "El formato de imagen no es compatible."
        )
    };
}
