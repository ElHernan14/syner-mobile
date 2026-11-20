namespace Syner.Api.Services.Archivos;

public interface IAlmacenamientoImagenesService
{
    Task<string> SubirImagenLoteAsync(
        IFormFile imagen,
        CancellationToken cancellationToken);
}
