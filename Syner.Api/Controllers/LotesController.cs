using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syner.Api.Data;
using Syner.Api.Data.Responses;
using Syner.Api.Data.Validation;
using Syner.Api.Domain.DTOs.Lotes;
using Syner.Api.Domain.Entities;
using Syner.Api.Domain.Enums;
using Syner.Api.Domain.States;

namespace Syner.Api.Controllers;

[ApiController]
[Route("api/lotes")]
public sealed class LotesController : ControllerBase
{
    private readonly SynerDbContext _db;

    public LotesController(SynerDbContext db)
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

        var consulta = _db.Lotes
            .AsNoTracking()
            .AsQueryable();

        // Filtro por término
        if (!string.IsNullOrWhiteSpace(paginacion.Termino))
        {
            var termino = paginacion.Termino.ToLower().Trim();

            consulta = consulta.Where(lote =>
                lote.Nombre.ToLower().Contains(termino) ||
                lote.Descripcion.ToLower().Contains(termino) ||
                lote.Categoria.ToLower().Contains(termino));
        }

        // Ordenamiento
        consulta = paginacion.Sort?.ToLower() switch
        {
            "nombre" =>
                consulta.OrderBy(lote => lote.Nombre),

            "nombre_desc" =>
                consulta.OrderByDescending(lote => lote.Nombre),

            "precio" =>
                consulta.OrderBy(lote => lote.PrecioCupo),

            "precio_desc" =>
                consulta.OrderByDescending(lote => lote.PrecioCupo),

            "fecha_inicio" =>
                consulta.OrderBy(lote => lote.FechaInicio),

            "fecha_inicio_desc" =>
                consulta.OrderByDescending(lote => lote.FechaInicio),

            "estado" =>
                consulta.OrderBy(lote => lote.Estado),

            "estado_desc" =>
                consulta.OrderByDescending(lote => lote.Estado),

            _ =>
                consulta.OrderBy(lote => lote.Id)
        };

        // Total antes de Skip/Take
        var total = await consulta.CountAsync(cancellationToken);

        // Offset calculado por el servidor
        var offset = paginacion.CalcularOffset();

        var lotes = await consulta
            .Skip(offset)
            .Take(paginacion.Limit)
            .Select(lote => new
            {
                id = lote.Id,
                nombre = lote.Nombre,
                descripcion = lote.Descripcion,
                categoria = lote.Categoria,
                precio_mercado = (double)lote.PrecioMercado,
                precio_cupo = (double)lote.PrecioCupo,
                porcentaje_ahorro = (double)lote.PorcentajeAhorro,
                cantidad_cupos = lote.CantidadCupos,
                cupos_ocupados = lote.CuposOcupados,
                fecha_inicio = lote.FechaInicio,
                fecha_fin = lote.FechaFin,
                estado = lote.Estado,
                proveedor = new
                {
                    id = lote.Proveedor.Id,
                    nombre = lote.Proveedor.Nombre,
                    descripcion = lote.Proveedor.Descripcion,
                    verificado = lote.Proveedor.Verificado
                }
            })
            .ToListAsync(cancellationToken);

        var resultado = new ResultadoPaginado<object>
        {
            Datos = lotes,
            Page = paginacion.Page,
            Limit = paginacion.Limit,
            Offset = offset,
            Total = total
        };

        return RespuestaHttp.Ok(
            this,
            "Lotes obtenidos correctamente.",
            resultado
        );
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        CrearLoteRequest request,
        CancellationToken cancellationToken)
    {
        // Validación de ModelState
        var validacionModelState = ValidadorModelState.Validar(ModelState);

        if (!validacionModelState.EsValido)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Los datos enviados no son válidos.",
                validacionModelState.Errores.ToArray()
            );
        }

        // Validación del DTO
        var validacion = request.Validar();

        if (!validacion.EsValido)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Los datos enviados no son válidos.",
                validacion.Errores.ToArray()
            );
        }

        // Validación de existencia del proveedor
        var proveedorExiste = await _db.Proveedores
            .AnyAsync(
                proveedor => proveedor.Id == request.ProveedorId,
                cancellationToken
            );

        if (!proveedorExiste)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El proveedor indicado no existe."
            );
        }

        // Validación de fechas
        if (request.FechaFin <= request.FechaInicio)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "La fecha de fin debe ser posterior a la fecha de inicio."
            );
        }

        // Validación de precios
        if (request.PrecioCupo >= request.PrecioMercado)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El precio de cupo debe ser menor al precio de mercado."
            );
        }

        var lote = new Lote
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Categoria = request.Categoria,
            PrecioMercado = request.PrecioMercado,
            PrecioCupo = request.PrecioCupo,
            PorcentajeAhorro = request.PorcentajeAhorro,
            CantidadCupos = request.CantidadCupos,
            CuposOcupados = 0,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            Estado = EstadoLote.Borrador
                .ToString()
                .ToLowerInvariant(),
            ProveedorId = request.ProveedorId
        };

        _db.Lotes.Add(lote);

        await _db.SaveChangesAsync(cancellationToken);

        return RespuestaHttp.Created(
            this,
            "Lote creado correctamente.",
            lote,
            nameof(ObtenerPorId),
            new { id = lote.Id }
        );
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var lote = await _db.Lotes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                lote => lote.Id == id,
                cancellationToken
            );

        if (lote is null)
        {
            return RespuestaHttp.NotFound<object>(
                this,
                "El lote no existe."
            );
        }

        return RespuestaHttp.Ok(
            this,
            "Lote obtenido correctamente.",
            lote
        );
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id,
        ActualizarLoteRequest request,
        CancellationToken cancellationToken)
    {
        // Validación de ModelState
        var validacionModelState = ValidadorModelState.Validar(ModelState);

        if (!validacionModelState.EsValido)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Los datos enviados no son válidos.",
                validacionModelState.Errores.ToArray()
            );
        }

        // Validación del DTO
        var validacion = request.Validar();

        if (!validacion.EsValido)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Los datos enviados no son válidos.",
                validacion.Errores.ToArray()
            );
        }

        var lote = await _db.Lotes
            .FirstOrDefaultAsync(
                lote => lote.Id == id,
                cancellationToken
            );

        if (lote is null)
        {
            return RespuestaHttp.NotFound<object>(
                this,
                "El lote no existe."
            );
        }

        // Validación del estado mediante la máquina de estados
        if (!Enum.TryParse<EstadoLote>(
                lote.Estado,
                ignoreCase: true,
                out var estadoActual))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El lote tiene un estado inválido."
            );
        }

        if (!ReglasEstado.Lotes.TryGetValue(
                estadoActual,
                out var operacionesPermitidas) ||
            !operacionesPermitidas.Contains(OperacionEstado.Modificacion))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                $"No se puede modificar un lote en estado '{lote.Estado}'."
            );
        }

        // Validación del proveedor
        var proveedorExiste = await _db.Proveedores
            .AnyAsync(
                proveedor => proveedor.Id == request.ProveedorId,
                cancellationToken
            );

        if (!proveedorExiste)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El proveedor indicado no existe."
            );
        }

        // Validación de fechas
        if (request.FechaFin <= request.FechaInicio)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "La fecha de fin debe ser posterior a la fecha de inicio."
            );
        }

        // Validación de precios
        if (request.PrecioCupo >= request.PrecioMercado)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El precio de cupo debe ser menor al precio de mercado."
            );
        }

        // Validación de cupos
        if (request.CantidadCupos < lote.CuposOcupados)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "La cantidad de cupos no puede ser menor a los cupos ocupados."
            );
        }

        lote.Nombre = request.Nombre;
        lote.Descripcion = request.Descripcion;
        lote.Categoria = request.Categoria;
        lote.PrecioMercado = request.PrecioMercado;
        lote.PrecioCupo = request.PrecioCupo;
        lote.PorcentajeAhorro = request.PorcentajeAhorro;
        lote.CantidadCupos = request.CantidadCupos;
        lote.FechaInicio = request.FechaInicio;
        lote.FechaFin = request.FechaFin;
        lote.ProveedorId = request.ProveedorId;

        await _db.SaveChangesAsync(cancellationToken);

        var resultado = new
        {
            id = lote.Id,
            nombre = lote.Nombre,
            descripcion = lote.Descripcion,
            categoria = lote.Categoria,
            precio_mercado = (double)lote.PrecioMercado,
            precio_cupo = (double)lote.PrecioCupo,
            porcentaje_ahorro = (double)lote.PorcentajeAhorro,
            cantidad_cupos = lote.CantidadCupos,
            cupos_ocupados = lote.CuposOcupados,
            fecha_inicio = lote.FechaInicio,
            fecha_fin = lote.FechaFin,
            estado = lote.Estado,
            proveedor_id = lote.ProveedorId
        };

        return RespuestaHttp.Ok(
            this,
            "Lote actualizado correctamente.",
            resultado
        );
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(
        int id,
        CancellationToken cancellationToken)
    {
        var lote = await _db.Lotes
            .FirstOrDefaultAsync(
                lote => lote.Id == id,
                cancellationToken
            );

        if (lote is null)
        {
            return RespuestaHttp.NotFound<object>(
                this,
                "El lote no existe."
            );
        }

        // Validación del estado mediante la máquina de estados
        if (!Enum.TryParse<EstadoLote>(
                lote.Estado,
                ignoreCase: true,
                out var estadoActual))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El lote tiene un estado inválido."
            );
        }

        if (!ReglasEstado.Lotes.TryGetValue(
                estadoActual,
                out var operacionesPermitidas) ||
            !operacionesPermitidas.Contains(OperacionEstado.Baja))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                $"No se puede dar de baja un lote en estado '{lote.Estado}'."
            );
        }

        // Baja lógica
        lote.Estado = EstadoLote.Cancelado
            .ToString()
            .ToLowerInvariant();

        await _db.SaveChangesAsync(cancellationToken);

        var resultado = new
        {
            id = lote.Id,
            estado = lote.Estado
        };

        return RespuestaHttp.Ok(
            this,
            "Lote dado de baja correctamente.",
            resultado
        );
    }
}