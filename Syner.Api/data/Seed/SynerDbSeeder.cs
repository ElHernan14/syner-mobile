using Microsoft.EntityFrameworkCore;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Data.Seed;

public static class SynerDbSeeder
{
    public static async Task SeedAsync(
        SynerDbContext db,
        CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Usuarios.AnyAsync(cancellationToken))
        {
            return;
        }

        var usuarios = new[]
        {
            new Usuario
            {
                Nombre = "Juan Pérez",
                Correo = "juan@example.com",
                Telefono = "+54 9 266 000000",
                Dni = "40123456",
                Rol = "usuario",
                Estado = "verificado"
            },
            new Usuario
            {
                Nombre = "María González",
                Correo = "maria@example.com",
                Telefono = "+54 9 266 111111",
                Dni = "41234567",
                Rol = "usuario",
                Estado = "pendiente"
            },
            new Usuario
            {
                Nombre = "Admin SYNER",
                Correo = "admin@syner.com",
                Telefono = "+54 9 266 222222",
                Dni = "38987654",
                Rol = "admin",
                Estado = "verificado"
            }
        };

        var proveedores = new[]
        {
            new Proveedor
            {
                Nombre = "Tech Mayorista",
                Descripcion = "Proveedor mayorista de tecnología.",
                Verificado = true
            },
            new Proveedor
            {
                Nombre = "Hogar Directo",
                Descripcion = "Proveedor mayorista de productos para el hogar.",
                Verificado = true
            },
            new Proveedor
            {
                Nombre = "Proveedor Pendiente",
                Descripcion = "Proveedor todavía en proceso de verificación.",
                Verificado = false
            }
        };

        db.Usuarios.AddRange(usuarios);
        db.Proveedores.AddRange(proveedores);

        await db.SaveChangesAsync(cancellationToken);

        var lotes = new[]
        {
            new Lote
            {
                Nombre = "Smartphone X",
                Descripcion = "Smartphone de última generación.",
                Categoria = "Tecnología",
                PrecioMercado = 1_000_000m,
                PrecioCupo = 600_000m,
                PorcentajeAhorro = 40m,
                CantidadCupos = 10,
                CuposOcupados = 7,
                FechaInicio = new DateTime(2026, 8, 1),
                FechaFin = new DateTime(2026, 9, 15),
                Estado = "fondeando",
                ProveedorId = proveedores[0].Id
            },
            new Lote
            {
                Nombre = "Notebook Pro",
                Descripcion = "Notebook profesional para trabajo y estudio.",
                Categoria = "Tecnología",
                PrecioMercado = 1_500_000m,
                PrecioCupo = 900_000m,
                PorcentajeAhorro = 40m,
                CantidadCupos = 10,
                CuposOcupados = 10,
                FechaInicio = new DateTime(2026, 7, 1),
                FechaFin = new DateTime(2026, 8, 20),
                Estado = "completado",
                ProveedorId = proveedores[0].Id
            },
            new Lote
            {
                Nombre = "Kit Hogar Premium",
                Descripcion = "Conjunto de productos esenciales para el hogar.",
                Categoria = "Hogar",
                PrecioMercado = 500_000m,
                PrecioCupo = 300_000m,
                PorcentajeAhorro = 40m,
                CantidadCupos = 10,
                CuposOcupados = 0,
                FechaInicio = new DateTime(2026, 8, 15),
                FechaFin = new DateTime(2026, 9, 30),
                Estado = "fondeando",
                ProveedorId = proveedores[1].Id
            }
        };

        db.Lotes.AddRange(lotes);

        await db.SaveChangesAsync(cancellationToken);

        var pedidos = new[]
        {
            new Pedido
            {
                UsuarioId = usuarios[0].Id,
                LoteId = lotes[0].Id,
                Estado = "enviado",
                NumeroSeguimiento = "SYNER-000001",
                CodigoEntrega = "482913"
            },
            new Pedido
            {
                UsuarioId = usuarios[0].Id,
                LoteId = lotes[1].Id,
                Estado = "completado",
                NumeroSeguimiento = "SYNER-000002",
                CodigoEntrega = null
            },
            new Pedido
            {
                UsuarioId = usuarios[1].Id,
                LoteId = lotes[2].Id,
                Estado = "fondeando",
                NumeroSeguimiento = null,
                CodigoEntrega = null
            }
        };

        db.Pedidos.AddRange(pedidos);

        await db.SaveChangesAsync(cancellationToken);
    }
}