using Syner.Api.Data;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Services.Lotes;

public sealed class HistorialLoteService(SynerDbContext db)
{
    public void RegistrarCambio(Lote lote, long usuarioId, string anterior, string nuevo)
    {
        db.HistorialLotes.Add(new HistorialLote
        {
            LoteId = lote.Id,
            UsuarioId = usuarioId,
            EstadoAnterior = anterior,
            NuevoEstado = nuevo,
            FechaModificacion = DateTime.UtcNow
        });
    }
}
