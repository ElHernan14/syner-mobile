namespace Syner.Api.Data.Validation;

public sealed class ResultadoValidacion
{
    public bool EsValido { get; init; }

    public IReadOnlyList<string> Errores { get; init; } = [];

    public static ResultadoValidacion Valido()
    {
        return new ResultadoValidacion
        {
            EsValido = true
        };
    }

    public static ResultadoValidacion Invalido(
        params string[] errores)
    {
        return new ResultadoValidacion
        {
            EsValido = false,
            Errores = errores
        };
    }
}