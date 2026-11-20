namespace Syner.Api.Domain.Entities;

public sealed class Proveedor : EntityBase
{
    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public bool Verificado { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }
}
