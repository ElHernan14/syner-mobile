using System.ComponentModel.DataAnnotations;

namespace Syner.Api.Data.Validation;

public static class ValidadorDto
{
    public static ResultadoValidacion Validar<T>(T modelo)
    {
        ArgumentNullException.ThrowIfNull(modelo);
        var contexto = new ValidationContext(modelo);

        var resultados = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(
            modelo,
            contexto,
            resultados,
            validateAllProperties: true
        );

        if (esValido)
        {
            return ResultadoValidacion.Valido();
        }

        var errores = resultados
            .Select(resultado => resultado.ErrorMessage)
            .Where(mensaje => !string.IsNullOrWhiteSpace(mensaje))
            .Cast<string>()
            .ToArray();

        return ResultadoValidacion.Invalido(errores);
    }
}