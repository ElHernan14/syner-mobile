using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Data.Configurations;

public sealed class DireccionConfiguration : IEntityTypeConfiguration<Direccion>
{
    public void Configure(EntityTypeBuilder<Direccion> builder)
    {
        builder.ToTable("direcciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.UsuarioId).IsRequired();
        builder.Property(x => x.Calle).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Numero).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Ciudad).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Provincia).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CodigoPostal).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Referencia).HasMaxLength(255).IsRequired(false);
        builder.Property(x => x.Latitud).HasPrecision(10, 7).IsRequired();
        builder.Property(x => x.Longitud).HasPrecision(10, 7).IsRequired();
        builder.Property(x => x.EsPrincipal).IsRequired();
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.FechaActualizacion).IsRequired();
        builder.Property(x => x.FechaEliminacion).IsRequired(false);

        builder.HasIndex(x => new { x.UsuarioId, x.EsPrincipal, x.FechaEliminacion });
        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.Direcciones)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
