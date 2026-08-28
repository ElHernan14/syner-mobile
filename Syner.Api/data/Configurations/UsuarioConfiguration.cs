using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Data.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Correo)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Telefono)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Dni)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Rol)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Estado)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => x.Correo)
            .IsUnique();

        builder.HasIndex(x => x.Dni)
            .IsUnique();
    }
}