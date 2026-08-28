using Microsoft.EntityFrameworkCore;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Data;

public sealed class SynerDbContext : DbContext
{
    public SynerDbContext(DbContextOptions<SynerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Proveedor> Proveedores => Set<Proveedor>();

    public DbSet<Lote> Lotes => Set<Lote>();

    public DbSet<Pedido> Pedidos => Set<Pedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SynerDbContext).Assembly
        );
    }
}