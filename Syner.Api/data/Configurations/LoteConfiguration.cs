using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Data.Configurations;

public sealed class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    public void Configure(EntityTypeBuilder<Lote> builder)
    {
        builder.ToTable("lotes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Descripcion)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Categoria)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PrecioMercado)
            .HasPrecision(15, 2)
            .IsRequired();

        builder.Property(x => x.PrecioCupo)
            .HasPrecision(15, 2)
            .IsRequired();

        builder.Property(x => x.PorcentajeAhorro)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.CantidadCupos)
            .IsRequired();

        builder.Property(x => x.CuposOcupados)
            .IsRequired();

        builder.Property(x => x.FechaInicio)
            .IsRequired();

        builder.Property(x => x.FechaFin)
            .IsRequired();

        builder.Property(x => x.Estado)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ProveedorId)
            .IsRequired();

        builder.HasOne(x => x.Proveedor)
            .WithMany()
            .HasForeignKey(x => x.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}