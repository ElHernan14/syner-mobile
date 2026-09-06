using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Syner.Api.Data.Validation;

public static class ValidadorModelState
{
    public static ResultadoValidacion Validar(
        ModelStateDictionary modelState)
    {
        if (modelState.IsValid)
        {
            return ResultadoValidacion.Valido();
        }

        var errores = modelState
            .Values
            .SelectMany(valor => valor.Errors)
            .Select(error =>
                !string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? error.ErrorMessage
                    : "El valor enviado no tiene un formato válido."
            )
            .Where(mensaje => !string.IsNullOrWhiteSpace(mensaje))
            .Distinct()
            .ToArray();

        return ResultadoValidacion.Invalido(errores);
    }
}