using Microsoft.AspNetCore.Mvc;

namespace Syner.Api.Data.Responses;

public static class RespuestaHttp
{
    public static IActionResult Ok<T>(
        ControllerBase controller,
        string mensaje,
        T? datos = default)
    {
        return controller.Ok(
            RespuestaApi<T>.Ok(mensaje, datos)
        );
    }

    public static IActionResult Created<T>(
        ControllerBase controller,
        string mensaje,
        T datos,
        string actionName,
        object routeValues)
    {
        return controller.CreatedAtAction(
            actionName,
            routeValues,
            RespuestaApi<T>.Ok(mensaje, datos)
        );
    }

    public static IActionResult BadRequest<T>(
        ControllerBase controller,
        string mensaje,
        params string[] errores)
    {
        return controller.BadRequest(
            RespuestaApi<T>.Error(mensaje, errores)
        );
    }

    public static IActionResult NotFound<T>(
        ControllerBase controller,
        string mensaje,
        params string[] errores)
    {
        return controller.NotFound(
            RespuestaApi<T>.Error(mensaje, errores)
        );
    }

    public static IActionResult Conflict<T>(
        ControllerBase controller,
        string mensaje,
        params string[] errores)
    {
        return controller.Conflict(
            RespuestaApi<T>.Error(mensaje, errores)
        );
    }
}