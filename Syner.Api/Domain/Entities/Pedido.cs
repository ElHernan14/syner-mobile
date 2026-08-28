namespace Syner.Api.Domain.Entities;

public sealed class Pedido : EntityBase
{
    public long UsuarioId { get; set; }

    public long LoteId { get; set; }

    public string Estado { get; set; } = "fondeando";

    public string? NumeroSeguimiento { get; set; }

    public string? CodigoEntrega { get; set; }
}