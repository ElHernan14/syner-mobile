namespace Syner.Api.Data.Responses;

public sealed class RespuestaApi<T>
{
    public bool Exito { get; init; }

    public string Mensaje { get; init; } = string.Empty;

    public T? Datos { get; init; }

    public IReadOnlyList<string> Errores { get; init; } = [];

    public static RespuestaApi<T> Ok(
        string mensaje,
        T? datos = default)
    {
        return new RespuestaApi<T>
        {
            Exito = true,
            Mensaje = mensaje,
            Datos = datos
        };
    }

    public static RespuestaApi<T> Error(
        string mensaje,
        params string[] errores)
    {
        return new RespuestaApi<T>
        {
            Exito = false,
            Mensaje = mensaje,
            Errores = errores
        };
    }
}