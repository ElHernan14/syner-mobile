using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Syner.Api.Domain.Entities;

namespace Syner.Api.Data.Configurations;

public sealed class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Nombre)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Descripcion)
            .HasMaxLength(250)
            .IsRequired();

        builder.HasIndex(x => x.Nombre)
            .IsUnique();
    }
}