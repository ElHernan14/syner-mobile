using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Data.Configurations;

public sealed class HistorialLoteConfiguration : IEntityTypeConfiguration<HistorialLote>
{
    public void Configure(EntityTypeBuilder<HistorialLote> builder)
    {
        builder.ToTable("historial_lotes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.EstadoAnterior).HasMaxLength(20).IsRequired();
        builder.Property(x => x.NuevoEstado).HasMaxLength(20).IsRequired();
        builder.Property(x => x.FechaModificacion).IsRequired();
        builder.HasIndex(x => new { x.LoteId, x.FechaModificacion });
        builder.HasIndex(x => x.UsuarioId);
        builder.HasOne(x => x.Lote).WithMany().HasForeignKey(x => x.LoteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}
