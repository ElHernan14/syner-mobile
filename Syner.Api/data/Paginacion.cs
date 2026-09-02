namespace Syner.Api.Data;

public sealed class Paginacion
{
    public int Page { get; init; } = 1;

    public int Limit { get; init; } = 10;

    public int Offset { get; init; } = 0;

    public string? Sort { get; init; }

    public string? Termino { get; init; }

    public void Validar()
    {
        if (Page < 1)
            throw new ArgumentException("El parámetro page debe ser mayor o igual a 1.");

        if (Limit < 1 || Limit > 100)
            throw new ArgumentException("El parámetro limit debe estar entre 1 y 100.");

        if (Offset < 0)
            throw new ArgumentException("El parámetro offset no puede ser negativo.");
    }

    public int CalcularOffset()
    {
        return (Page - 1) * Limit;
    }
}

public sealed class ResultadoPaginado<T>
{
    public IReadOnlyList<T> Datos { get; init; } = [];

    public int Page { get; init; }

    public int Limit { get; init; }

    public int Offset { get; init; }

    public int Total { get; init; }

    public int TotalPaginas =>
        Limit > 0
            ? (int)Math.Ceiling((double)Total / Limit)
            : 0;
}