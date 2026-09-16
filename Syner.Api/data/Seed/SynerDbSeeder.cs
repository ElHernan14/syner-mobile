using Microsoft.EntityFrameworkCore;

using Syner.Api.Domain.Entities;

using Syner.Api.Services;

namespace Syner.Api.Data.Seed;

public static class SynerDbSeeder
{
    public static async Task SeedAsync(
        SynerDbContext db,
        PasswordService passwordService,
        CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);

        await SeedRolesAsync(db, cancellationToken);
        await SeedUsuariosAsync(
            db,
            passwordService,
            cancellationToken
        );

        await SeedDatosDominioAsync(db, cancellationToken);
    }

    private static async Task SeedRolesAsync(
        SynerDbContext db,
        CancellationToken cancellationToken)
    {
        var roles = new[]
        {
            new Rol
            {
                Nombre = "admin",
                Descripcion = "Administrador con acceso completo al sistema."
            },
            new Rol
            {
                Nombre = "usuario",
                Descripcion = "Usuario autenticado con acceso a las funcionalidades disponibles."
            }
        };

        foreach (var rol in roles)
        {
            var existe = await db.Roles
                .AnyAsync(x => x.Nombre == rol.Nombre, cancellationToken);

            if (!existe)
            {
                db.Roles.Add(rol);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedUsuariosAsync(
        SynerDbContext db,
        PasswordService passwordService,
        CancellationToken cancellationToken)
    {
        var rolAdmin = await db.Roles
            .SingleAsync(x => x.Nombre == "admin", cancellationToken);

        var rolUsuario = await db.Roles
            .SingleAsync(x => x.Nombre == "usuario", cancellationToken);

        var usuarios = new[]
        {
            new Usuario
            {
                Nombre = "Juan Pérez",
                Correo = "juan@example.com",
                Telefono = "+54 9 266 000000",
                Dni = "40123456",
                RolId = rolUsuario.Id,
                PasswordHash = passwordService.Hash("Juan123!"),
                Estado = "verificado"
            },
            new Usuario
            {
                Nombre = "María González",
                Correo = "maria@example.com",
                Telefono = "+54 9 266 111111",
                Dni = "41234567",
                RolId = rolUsuario.Id,
                PasswordHash = passwordService.Hash("Maria123!"),
                Estado = "pendiente"
            },
            new Usuario
            {
                Nombre = "Admin SYNER",
                Correo = "admin@syner.com",
                Telefono = "+54 9 266 222222",
                Dni = "38987654",
                RolId = rolAdmin.Id,
                PasswordHash = passwordService.Hash("Admin123!"),
                Estado = "verificado"
            },
            new Usuario
            {
                Nombre = "Usuario Sin Rol",
                Correo = "sinrol@example.com",
                Telefono = "+54 9 266 333333",
                Dni = "42345678",
                RolId = null,
                PasswordHash = passwordService.Hash("SinRol123!"),
                Estado = "verificado"
            }
        };

        foreach (var usuarioSeed in usuarios)
        {
            var usuarioExistente = await db.Usuarios
                .FirstOrDefaultAsync(
                    x => x.Correo == usuarioSeed.Correo,
                    cancellationToken
                );

            if (usuarioExistente is null)
            {
                db.Usuarios.Add(usuarioSeed);
                continue;
            }

            if (string.IsNullOrWhiteSpace(usuarioExistente.PasswordHash))
            {
                usuarioExistente.PasswordHash = usuarioSeed.PasswordHash;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDatosDominioAsync(
    SynerDbContext db,
    CancellationToken cancellationToken)
{
    if (await db.Proveedores.AnyAsync(cancellationToken))
    {
        return;
    }

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

    var usuarioJuan = await db.Usuarios
        .SingleAsync(
            x => x.Correo == "juan@example.com",
            cancellationToken
        );

    var usuarioMaria = await db.Usuarios
        .SingleAsync(
            x => x.Correo == "maria@example.com",
            cancellationToken
        );

    var pedidos = new[]
    {
        new Pedido
        {
            UsuarioId = usuarioJuan.Id,
            LoteId = lotes[0].Id,
            Estado = "enviado",
            NumeroSeguimiento = "SYNER-000001",
            CodigoEntrega = "482913"
        },
        new Pedido
        {
            UsuarioId = usuarioJuan.Id,
            LoteId = lotes[1].Id,
            Estado = "completado",
            NumeroSeguimiento = "SYNER-000002",
            CodigoEntrega = null
        },
        new Pedido
        {
            UsuarioId = usuarioMaria.Id,
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