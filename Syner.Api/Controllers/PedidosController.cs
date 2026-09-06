using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syner.Api.Data;
using Syner.Api.Data.Responses;
using Syner.Api.Data.Validation;
using Syner.Api.Domain.DTOs.Pedidos;
using Syner.Api.Domain.Entities;
using Syner.Api.Domain.Enums;
using Syner.Api.Domain.States;

namespace Syner.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
public sealed class PedidosController : ControllerBase
{
    private readonly SynerDbContext _db;

    public PedidosController(SynerDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] Paginacion paginacion,
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

            _ => consulta.OrderBy(
                pedido => pedido.Id)
        };

        var total = await consulta.CountAsync(cancellationToken);

        var offset = paginacion.CalcularOffset();

        var pedidos = await consulta
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
                    precio_cupo = (double)pedido.Lote.PrecioCupo
                }
            })
            .ToListAsync(cancellationToken);

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

        var pedido = new Pedido
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
}