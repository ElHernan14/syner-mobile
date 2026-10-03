using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Data.Configurations;

public sealed class DispositivoConfiguration
    : IEntityTypeConfiguration<Dispositivo>
{
    public void Configure(EntityTypeBuilder<Dispositivo> builder)
    {
        builder.ToTable("dispositivos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UsuarioId)
            .IsRequired();

        builder.Property(x => x.Token)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Plataforma)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Activo)
            .IsRequired();

        builder.Property(x => x.FechaRegistro)
            .IsRequired();

        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.Dispositivos)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.UsuarioId,
            x.Activo
        });
    }
}