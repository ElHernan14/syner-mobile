namespace Syner.Api.Domain.Entities;

public sealed class RefreshToken : EntityBase
{
    public long UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreadoEn { get; set; }

    public DateTime ExpiraEn { get; set; }

    public DateTime? RevocadoEn { get; set; }

    public bool EstaExpirado =>
        DateTime.UtcNow >= ExpiraEn;

    public bool EstaRevocado =>
        RevocadoEn.HasValue;

    public bool EsValido =>
        !EstaExpirado &&
        !EstaRevocado;
}