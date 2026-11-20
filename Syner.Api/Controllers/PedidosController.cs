using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syner.Api.Data;
using Syner.Api.Data.Responses;
using Syner.Api.Data.Validation;
using Syner.Api.Domain.DTOs.Pedidos;
using Syner.Api.Domain.Enums;
using Syner.Api.Domain.States;
using Syner.Api.Services.Notificaciones;
using Syner.Api.Services.Lotes;

namespace Syner.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/pedidos")]
public sealed class PedidosController : ControllerBase
{
    private readonly SynerDbContext _db;
    private readonly NotificacionService _notificacion_service;
    private readonly NotificacionPushService _notificacion_push_service;
    private readonly IWebHostEnvironment _environment;
    private readonly LoteQrService _qr_service;

    public PedidosController(
        SynerDbContext db,
        NotificacionService notificacion_service,
        NotificacionPushService notificacion_push_service,
        IWebHostEnvironment environment,
        LoteQrService qr_service)
    {
        _db = db;
        _notificacion_service = notificacion_service;
        _notificacion_push_service = notificacion_push_service;
        _environment = environment;
        _qr_service = qr_service;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] Paginacion paginacion,
        [FromQuery] long? pedidoId,
        CancellationToken cancellationToken)
    {
        try
        {
            paginacion.Validar();
        }
        catch (ArgumentException ex)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Los parámetros de paginación no son válidos.",
                ex.Message
            );
        }

        var consulta = _db.Pedidos
            .AsNoTracking()
            .AsQueryable();

        // Un usuario solo puede consultar sus propios pedidos. El filtro de
        // pedidoId nunca debe permitir atravesar esta restricción.
        if (!User.IsInRole("admin"))
        {
            var usuario_id = ObtenerUsuarioId();
            if (usuario_id is null)
            {
                return Unauthorized();
            }

            consulta = consulta.Where(pedido => pedido.UsuarioId == usuario_id.Value);
        }

        if (pedidoId.HasValue)
        {
            if (pedidoId.Value <= 0)
            {
                return RespuestaHttp.BadRequest<object>(
                    this,
                    "El identificador del pedido no es válido."
                );
            }

            consulta = consulta.Where(pedido => pedido.Id == pedidoId.Value);
        }

        if (!string.IsNullOrWhiteSpace(paginacion.Termino))
        {
            var termino = paginacion.Termino.ToLower().Trim();

            consulta = consulta.Where(pedido =>
                (pedido.NumeroSeguimiento != null &&
                 pedido.NumeroSeguimiento.ToLower().Contains(termino)) ||

                (pedido.CodigoEntrega != null &&
                 pedido.CodigoEntrega.ToLower().Contains(termino)) ||

                pedido.Usuario.Nombre.ToLower().Contains(termino) ||

                pedido.Usuario.Correo.ToLower().Contains(termino) ||

                pedido.Lote.Nombre.ToLower().Contains(termino) ||

                pedido.Lote.Categoria.ToLower().Contains(termino));
        }

        consulta = paginacion.Sort?.ToLower() switch
        {
            "estado" => consulta.OrderBy(
                pedido => pedido.Estado),

            "estado_desc" => consulta.OrderByDescending(
                pedido => pedido.Estado),

            "usuario" => consulta.OrderBy(
                pedido => pedido.Usuario.Nombre),

            "usuario_desc" => consulta.OrderByDescending(
                pedido => pedido.Usuario.Nombre),

            "lote" => consulta.OrderBy(
                pedido => pedido.Lote.Nombre),

            "lote_desc" => consulta.OrderByDescending(
                pedido => pedido.Lote.Nombre),

            _ => consulta.OrderByDescending(
                pedido => pedido.Id)
        };

        var total = await consulta.CountAsync(cancellationToken);

        var offset = paginacion.CalcularOffset();

        var pedidos_base = await consulta
            .Skip(offset)
            .Take(paginacion.Limit)
            .Select(pedido => new
            {
                id = pedido.Id,
                estado = pedido.Estado,
                numero_seguimiento = pedido.NumeroSeguimiento,
                codigo_entrega = pedido.CodigoEntrega,

                usuario = new
                {
                    id = pedido.Usuario.Id,
                    nombre = pedido.Usuario.Nombre,
                    correo = pedido.Usuario.Correo
                },

                lote = new
                {
                    id = pedido.Lote.Id,
                    nombre = pedido.Lote.Nombre,
                    categoria = pedido.Lote.Categoria,
                    precio_cupo = pedido.Lote.PrecioCupo,
                    precio_mercado = pedido.Lote.PrecioMercado,
                    proveedor = pedido.Lote.Proveedor.Nombre,
                    proveedor_verificado = pedido.Lote.Proveedor.Verificado
                }
            })
            .ToListAsync(cancellationToken);

        var pedidos = pedidos_base.Select(pedido => new
        {
            pedido.id,
            pedido.estado,
            pedido.numero_seguimiento,
            pedido.codigo_entrega,
            pedido.usuario,
            lote = new
            {
                pedido.lote.id,
                pedido.lote.nombre,
                pedido.lote.categoria,
                precio_cupo = (double)pedido.lote.precio_cupo,
                precio_mercado = (double)pedido.lote.precio_mercado,
                porcentaje_ahorro = pedido.lote.precio_mercado > 0
                    ? (double)Math.Round((pedido.lote.precio_mercado - pedido.lote.precio_cupo) / pedido.lote.precio_mercado * 100m, 2, MidpointRounding.AwayFromZero)
                    : 0d,
                pedido.lote.proveedor,
                pedido.lote.proveedor_verificado
            }
        }).ToList();

        var resultado = new ResultadoPaginado<object>
        {
            Datos = pedidos,
            Page = paginacion.Page,
            Limit = paginacion.Limit,
            Offset = offset,
            Total = total
        };

        return RespuestaHttp.Ok(
            this,
            "Pedidos obtenidos correctamente.",
            resultado
        );
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Crear(
        CrearPedidoRequest request,
        CancellationToken cancellationToken)
    {
        var validacionModelState = ValidadorModelState.Validar(ModelState);

        if (!validacionModelState.EsValido)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Los datos enviados no son válidos.",
                validacionModelState.Errores.ToArray()
            );
        }

        var validacion = request.Validar();

        if (!validacion.EsValido)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Los datos enviados no son válidos.",
                validacion.Errores.ToArray()
            );
        }

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                usuario => usuario.Id == request.UsuarioId,
                cancellationToken
            );

        if (usuario is null)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El usuario indicado no existe."
            );
        }

        var lote = await _db.Lotes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                lote => lote.Id == request.LoteId,
                cancellationToken
            );

        if (lote is null)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El lote indicado no existe."
            );
        }

        var pedido = new Domain.Entities.Pedido
        {
            UsuarioId = request.UsuarioId,
            LoteId = request.LoteId,
            Estado = EstadoPedido.Fondeando
                .ToString()
                .ToLowerInvariant(),
            NumeroSeguimiento = request.NumeroSeguimiento,
            CodigoEntrega = request.CodigoEntrega
        };

        _db.Pedidos.Add(pedido);

        await _db.SaveChangesAsync(cancellationToken);

        var resultado = new
        {
            id = pedido.Id,
            estado = pedido.Estado,
            numero_seguimiento = pedido.NumeroSeguimiento,
            codigo_entrega = pedido.CodigoEntrega,

            usuario = new
            {
                id = usuario.Id,
                nombre = usuario.Nombre,
                correo = usuario.Correo
            },

            lote = new
            {
                id = lote.Id,
                nombre = lote.Nombre,
                categoria = lote.Categoria,
                precio_cupo = (double)lote.PrecioCupo
            }
        };

        return RespuestaHttp.Created(
            this,
            "Pedido creado correctamente.",
            resultado,
            nameof(ObtenerPorId),
            new { id = pedido.Id }
        );
    }

    [HttpPost("cupo")]
    [Authorize(Roles = "usuario")]
    public async Task<IActionResult> AdquirirCupo(
        [FromForm] CrearPedidoCupoRequest request,
        CancellationToken cancellationToken)
    {
        var validacionModelState = ValidadorModelState.Validar(ModelState);

        if (!validacionModelState.EsValido)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Los datos enviados no son válidos.",
                validacionModelState.Errores.ToArray()
            );
        }

        var validacion = request.Validar();

        if (!validacion.EsValido)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Los datos enviados no son válidos.",
                validacion.Errores.ToArray()
            );
        }

        var usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "No se pudo identificar al usuario autenticado."
            );
        }

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                usuario => usuario.Id == usuarioId.Value,
                cancellationToken
            );

        if (usuario is null)
        {
            return RespuestaHttp.NotFound<object>(
                this,
                "El usuario autenticado no existe."
            );
        }

        if (!string.Equals(
                usuario.Estado,
                "verificado",
                StringComparison.OrdinalIgnoreCase))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El usuario debe estar verificado para adquirir un cupo."
            );
        }

        var lote = await _db.Lotes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                lote => lote.Id == request.LoteId,
                cancellationToken
            );

        if (lote is null)
        {
            return RespuestaHttp.NotFound<object>(
                this,
                "El lote indicado no existe."
            );
        }

        if (!string.Equals(
                lote.Estado,
                "fondeando",
                StringComparison.OrdinalIgnoreCase))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El lote no está disponible para adquirir un cupo."
            );
        }

        var pedidoExistente = await _db.Pedidos
            .AsNoTracking()
            .AnyAsync(
                pedido =>
                    pedido.UsuarioId == usuarioId.Value &&
                    pedido.LoteId == request.LoteId &&
                    pedido.Estado.ToLower() != "cancelado",
                cancellationToken
            );

        if (pedidoExistente)
        {
            return RespuestaHttp.Conflict<object>(
                this,
                "El usuario ya tiene un pedido activo para este lote."
            );
        }

        await using (var validacionFirma = request.Firma!.OpenReadStream())
        {
            var cabecera = new byte[8];
            var leidos = await validacionFirma.ReadAsync(cabecera, cancellationToken);
            var cabeceraPng = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
            if (leidos != cabecera.Length || !cabecera.SequenceEqual(cabeceraPng))
                return RespuestaHttp.BadRequest<object>(this, "La firma enviada no es un archivo PNG válido.");
        }

        await using var transaccion = await _db.Database
        .BeginTransactionAsync(cancellationToken);

        string? rutaArchivoFirma = null;
        try
        {
            var cupoActualizado = await _db.Lotes
                .Where(lote =>
                    lote.Id == request.LoteId &&
                    lote.Estado.ToLower() == "fondeando" &&
                    lote.CuposOcupados < lote.CantidadCupos)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        lote => lote.CuposOcupados,
                        lote => lote.CuposOcupados + 1),
                    cancellationToken
                );

            if (cupoActualizado == 0)
            {
                await transaccion.RollbackAsync(cancellationToken);

                return RespuestaHttp.Conflict<object>(
                    this,
                    "No quedan cupos disponibles para este lote."
                );
            }

            var pedido = new Domain.Entities.Pedido
            {
                UsuarioId = usuarioId.Value,
                LoteId = request.LoteId,
                Estado = EstadoPedido.Fondeando
                    .ToString()
                    .ToLowerInvariant()
            };

            var directorioFirmas = Path.Combine(_environment.ContentRootPath, "App_Data", "firmas_pedidos");
            Directory.CreateDirectory(directorioFirmas);
            var nombreFirma = $"{Guid.NewGuid():N}.png";
            var rutaFirma = Path.Combine(directorioFirmas, nombreFirma);
            rutaArchivoFirma = rutaFirma;
            await using (var firmaStream = request.Firma!.OpenReadStream())
            await using (var archivoStream = new FileStream(rutaFirma, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await firmaStream.CopyToAsync(archivoStream, cancellationToken);
            }
            pedido.FirmaPath = $"firmas_pedidos/{nombreFirma}";

            _db.Pedidos.Add(pedido);

            await _db.SaveChangesAsync(cancellationToken);

            var administradores = await _db.Usuarios
                .Where(usuario =>
                    usuario.Rol != null &&
                    usuario.Rol.Nombre.ToLower() == "admin")
                .Select(usuario => usuario.Id)
                .ToListAsync(cancellationToken);

            foreach (var administradorId in administradores)
            {
                await _notificacion_service.CrearAsync(
                    administradorId,
                    "pedido_nuevo",
                    "Nuevo cupo adquirido",
                    $"{usuario.Nombre} adquirió un cupo en el lote '{lote.Nombre}'.",
                    "pedido",
                    pedido.Id,
                    cancellationToken
                );
            }

            await _notificacion_service.CrearAsync(
                usuarioId.Value,
                "pedido_creado",
                "Cupo solicitado",
                $"Tu cupo para el lote '{lote.Nombre}' fue registrado correctamente.",
                "pedido",
                pedido.Id,
                cancellationToken
            );

            await transaccion.CommitAsync(cancellationToken);

            await _notificacion_push_service.EnviarAUsuariosAsync(
                administradores,
                "Nuevo cupo adquirido",
                $"{usuario.Nombre} adquirió un cupo en el lote '{lote.Nombre}'.",
                "pedido_nuevo",
                "pedido",
                pedido.Id,
                cancellationToken
            );

            // Cada notificación persistida también debe llegar por push al
            // mismo destinatario, incluida la confirmación del comprador.
            await _notificacion_push_service.EnviarAUsuariosAsync(
                [usuarioId.Value],
                "Cupo solicitado",
                $"Tu cupo para el lote '{lote.Nombre}' fue registrado correctamente.",
                "pedido_creado",
                "pedido",
                pedido.Id,
                cancellationToken
            );

            var resultado = new
            {
                id = pedido.Id,
                estado = pedido.Estado,
                usuario = new
                {
                    id = usuario.Id,
                    nombre = usuario.Nombre,
                    correo = usuario.Correo
                },
                lote = new
                {
                    id = lote.Id,
                    nombre = lote.Nombre,
                    categoria = lote.Categoria,
                    precio_cupo = (double)lote.PrecioCupo
                }
            };

            return RespuestaHttp.Created(
                this,
                "Cupo adquirido correctamente.",
                resultado,
                nameof(ObtenerPorId),
                new { id = pedido.Id }
            );
        }
        catch
        {
            await transaccion.RollbackAsync(cancellationToken);
            if (rutaArchivoFirma is not null && System.IO.File.Exists(rutaArchivoFirma)) System.IO.File.Delete(rutaArchivoFirma);
            throw;
        }
    }

    [HttpGet("{id:long}/firma")]
    public async Task<IActionResult> ObtenerFirma(long id, CancellationToken cancellationToken)
    {
        var pedido = await _db.Pedidos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (pedido is null || string.IsNullOrWhiteSpace(pedido.FirmaPath)) return NotFound();
        var usuarioId = ObtenerUsuarioId();
        if (!User.IsInRole("admin") && usuarioId != pedido.UsuarioId) return Forbid();
        var ruta = Path.Combine(_environment.ContentRootPath, "App_Data", pedido.FirmaPath.Replace('/', Path.DirectorySeparatorChar));
        if (!System.IO.File.Exists(ruta)) return NotFound();
        return PhysicalFile(ruta, "image/png", enableRangeProcessing: false);
    }

    [HttpGet("{id:long}/qr")]
    public async Task<IActionResult> ObtenerQrPublico(long id, CancellationToken cancellationToken)
    {
        var pedido = await _db.Pedidos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (pedido is null) return NotFound();
        var usuario_id = ObtenerUsuarioId();
        if (!User.IsInRole("admin") && usuario_id != pedido.UsuarioId) return Forbid();
        return File(_qr_service.GenerarPedido(pedido.TokenPublico), "image/png", $"syner-pedido-{id}.png");
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> ObtenerPorId(
        long id,
        CancellationToken cancellationToken)
    {
        var pedido = await _db.Pedidos
            .AsNoTracking()
            .Where(pedido => pedido.Id == id)
            .Select(pedido => new
            {
                id = pedido.Id,
                estado = pedido.Estado,
                numero_seguimiento = pedido.NumeroSeguimiento,
                codigo_entrega = pedido.CodigoEntrega,

                usuario = new
                {
                    id = pedido.Usuario.Id,
                    nombre = pedido.Usuario.Nombre,
                    correo = pedido.Usuario.Correo
                },

                lote = new
                {
                    id = pedido.Lote.Id,
                    nombre = pedido.Lote.Nombre,
                    categoria = pedido.Lote.Categoria,
                    precio_cupo = (double)pedido.Lote.PrecioCupo
                }
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (pedido is null)
        {
            return RespuestaHttp.NotFound<object>(
                this,
                "El pedido no existe."
            );
        }

        return RespuestaHttp.Ok(
            this,
            "Pedido obtenido correctamente.",
            pedido
        );
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Actualizar(
        long id,
        ActualizarPedidoRequest request,
        CancellationToken cancellationToken)
    {
        var validacionModelState = ValidadorModelState.Validar(ModelState);

        if (!validacionModelState.EsValido)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Los datos enviados no son válidos.",
                validacionModelState.Errores.ToArray()
            );
        }

        var validacion = request.Validar();

        if (!validacion.EsValido)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Los datos enviados no son válidos.",
                validacion.Errores.ToArray()
            );
        }

        var pedido = await _db.Pedidos
            .FirstOrDefaultAsync(
                pedido => pedido.Id == id,
                cancellationToken
            );

        if (pedido is null)
        {
            return RespuestaHttp.NotFound<object>(
                this,
                "El pedido no existe."
            );
        }

        if (!Enum.TryParse<EstadoPedido>(
                pedido.Estado,
                ignoreCase: true,
                out var estadoActual))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El pedido tiene un estado inválido."
            );
        }

        if (!ReglasEstado.Pedidos.TryGetValue(
                estadoActual,
                out var operacionesPermitidas) ||
            !operacionesPermitidas.Contains(
                OperacionEstado.Modificacion))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                $"No se puede modificar un pedido en estado '{pedido.Estado}'."
            );
        }

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                usuario => usuario.Id == request.UsuarioId,
                cancellationToken
            );

        if (usuario is null)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El usuario indicado no existe."
            );
        }

        var lote = await _db.Lotes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                lote => lote.Id == request.LoteId,
                cancellationToken
            );

        if (lote is null)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El lote indicado no existe."
            );
        }

        pedido.UsuarioId = request.UsuarioId;
        pedido.LoteId = request.LoteId;
        pedido.NumeroSeguimiento = request.NumeroSeguimiento;
        pedido.CodigoEntrega = request.CodigoEntrega;

        await _db.SaveChangesAsync(cancellationToken);

        var resultado = new
        {
            id = pedido.Id,
            estado = pedido.Estado,
            numero_seguimiento = pedido.NumeroSeguimiento,
            codigo_entrega = pedido.CodigoEntrega,

            usuario = new
            {
                id = usuario.Id,
                nombre = usuario.Nombre,
                correo = usuario.Correo
            },

            lote = new
            {
                id = lote.Id,
                nombre = lote.Nombre,
                categoria = lote.Categoria,
                precio_cupo = (double)lote.PrecioCupo
            }
        };

        return RespuestaHttp.Ok(
            this,
            "Pedido actualizado correctamente.",
            resultado
        );
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Eliminar(
        long id,
        CancellationToken cancellationToken)
    {
        var pedido = await _db.Pedidos
            .FirstOrDefaultAsync(
                pedido => pedido.Id == id,
                cancellationToken
            );

        if (pedido is null)
        {
            return RespuestaHttp.NotFound<object>(
                this,
                "El pedido no existe."
            );
        }

        if (!Enum.TryParse<EstadoPedido>(
                pedido.Estado,
                ignoreCase: true,
                out var estadoActual))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El pedido tiene un estado inválido."
            );
        }

        if (!ReglasEstado.Pedidos.TryGetValue(
                estadoActual,
                out var operacionesPermitidas) ||
            !operacionesPermitidas.Contains(
                OperacionEstado.Baja))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                $"No se puede dar de baja un pedido en estado '{pedido.Estado}'."
            );
        }

        pedido.Estado = EstadoPedido.Cancelado
            .ToString()
            .ToLowerInvariant();

        await _db.SaveChangesAsync(cancellationToken);

        var resultado = new
        {
            id = pedido.Id,
            estado = pedido.Estado
        };

        return RespuestaHttp.Ok(
            this,
            "Pedido dado de baja correctamente.",
            resultado
        );
    }

    private long? ObtenerUsuarioId()
    {
        var valor = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return long.TryParse(valor, out var usuarioId)
            ? usuarioId
            : null;
    }
}
