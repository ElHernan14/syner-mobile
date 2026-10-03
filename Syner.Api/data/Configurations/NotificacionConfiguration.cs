using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Data.Configurations;

public sealed class NotificacionConfiguration
    : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("notificaciones");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UsuarioId)
            .IsRequired();

        builder.Property(x => x.Tipo)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Titulo)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Mensaje)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Leida)
            .IsRequired();

        builder.Property(x => x.FechaCreacion)
            .IsRequired();

        builder.Property(x => x.EntidadTipo)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(x => x.EntidadId)
            .IsRequired(false);

        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.Notificaciones)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.UsuarioId,
            x.Leida
        });

        builder.HasIndex(x => x.FechaCreacion);
    }
}