using Microsoft.EntityFrameworkCore;
using Syner.Api.Data;

namespace Syner.Api.Services;

public sealed class DispositivoService
{
    private readonly SynerDbContext _db;

    public DispositivoService(SynerDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<string>> ObtenerTokensActivosAsync(
        IEnumerable<long> usuarioIds,
        CancellationToken cancellationToken = default)
    {
        var ids = usuarioIds
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return [];
        }

        return await _db.Dispositivos
            .AsNoTracking()
            .Where(x =>
                ids.Contains(x.UsuarioId) &&
                x.Activo
            )
            .Select(x => x.Token)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}