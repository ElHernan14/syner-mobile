using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;

using Syner.Api.Data;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Services;

public sealed class RefreshTokenService
{
    private const int TokenSizeBytes = 64;

    public string GenerarToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(
            TokenSizeBytes
        );

        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", string.Empty);
    }

    public string GenerarHash(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);

        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash)
            .ToLowerInvariant();
    }

    public RefreshToken Crear(
        Usuario usuario,
        string token,
        int duracionDias)
    {
        if (duracionDias <= 0)
        {
            throw new ArgumentException(
                "La duración del refresh token debe ser mayor a cero.",
                nameof(duracionDias)
            );
        }

        return new RefreshToken
        {
            UsuarioId = usuario.Id,
            Usuario = usuario,
            TokenHash = GenerarHash(token),
            CreadoEn = DateTime.UtcNow,
            ExpiraEn = DateTime.UtcNow.AddDays(duracionDias)
        };
    }

    public async Task LimpiarTokensInutilizables(
        SynerDbContext db,
        long usuarioId,
        CancellationToken cancellationToken)
    {
        var tokensInutilizables = await db.RefreshTokens
            .Where(x =>
                x.UsuarioId == usuarioId &&
                (
                    x.RevocadoEn.HasValue ||
                    x.ExpiraEn <= DateTime.UtcNow
                ))
            .ToListAsync(cancellationToken);

        if (tokensInutilizables.Count == 0)
        {
            return;
        }

        db.RefreshTokens.RemoveRange(
            tokensInutilizables
        );

        await db.SaveChangesAsync(cancellationToken);
    }
}