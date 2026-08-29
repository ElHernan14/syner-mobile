using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Data.Configurations;

public sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("pedidos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UsuarioId)
            .IsRequired();

        builder.Property(x => x.LoteId)
            .IsRequired();

        builder.Property(x => x.Estado)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.NumeroSeguimiento)
            .HasMaxLength(100);

        builder.Property(x => x.CodigoEntrega)
            .HasMaxLength(50);

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Lote)
            .WithMany()
            .HasForeignKey(x => x.LoteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}