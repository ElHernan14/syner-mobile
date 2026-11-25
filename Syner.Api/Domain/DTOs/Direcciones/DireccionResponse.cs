namespace Syner.Api.Domain.DTOs.Direcciones;

/// <summary>Dirección principal del usuario autenticado, incluida su ubicación.</summary>
public sealed record DireccionResponse(
    long Id,
    string Calle,
    string Numero,
    string Ciudad,
    string Provincia,
    string CodigoPostal,
    string? Referencia,
    decimal Latitud,
    decimal Longitud,
    bool EsPrincipal,
    DateTime FechaCreacion,
    DateTime FechaActualizacion);
