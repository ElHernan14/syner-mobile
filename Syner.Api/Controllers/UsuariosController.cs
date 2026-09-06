using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syner.Api.Data;
using Syner.Api.Data.Responses;
using Syner.Api.Data.Validation;
using Syner.Api.Domain.DTOs.Usuarios;
using Syner.Api.Domain.Entities;
using Syner.Api.Domain.Enums;
using Syner.Api.Domain.States;

namespace Syner.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public sealed class UsuariosController : ControllerBase
{
    private readonly SynerDbContext _db;

    public UsuariosController(SynerDbContext db)
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

        var consulta = _db.Usuarios
            .AsNoTracking()
            .AsQueryable();

        // Filtro por término
        if (!string.IsNullOrWhiteSpace(paginacion.Termino))
        {
            var termino = paginacion.Termino.ToLower().Trim();

            consulta = consulta.Where(usuario =>
                usuario.Nombre.ToLower().Contains(termino) ||
                usuario.Correo.ToLower().Contains(termino) ||
                usuario.Dni.ToLower().Contains(termino));
        }

        // Ordenamiento
        consulta = paginacion.Sort?.ToLower() switch
        {
            "nombre" =>
                consulta.OrderBy(usuario => usuario.Nombre),

            "nombre_desc" =>
                consulta.OrderByDescending(usuario => usuario.Nombre),

            "correo" =>
                consulta.OrderBy(usuario => usuario.Correo),

            "correo_desc" =>
                consulta.OrderByDescending(usuario => usuario.Correo),

            "dni" =>
                consulta.OrderBy(usuario => usuario.Dni),

            "dni_desc" =>
                consulta.OrderByDescending(usuario => usuario.Dni),

            "rol" =>
                consulta.OrderBy(usuario => usuario.Rol),

            "rol_desc" =>
                consulta.OrderByDescending(usuario => usuario.Rol),

            "estado" =>
                consulta.OrderBy(usuario => usuario.Estado),

            "estado_desc" =>
                consulta.OrderByDescending(usuario => usuario.Estado),

            _ =>
                consulta.OrderBy(usuario => usuario.Id)
        };

        // Total antes de Skip/Take
        var total = await consulta.CountAsync(cancellationToken);

        // Offset calculado por el servidor
        var offset = paginacion.CalcularOffset();

        var usuarios = await consulta
            .Skip(offset)
            .Take(paginacion.Limit)
            .Select(usuario => new
            {
                id = usuario.Id,
                nombre = usuario.Nombre,
                correo = usuario.Correo,
                telefono = usuario.Telefono,
                dni = usuario.Dni,
                rol = usuario.Rol,
                estado = usuario.Estado
            })
            .ToListAsync(cancellationToken);

        var resultado = new ResultadoPaginado<object>
        {
            Datos = usuarios,
            Page = paginacion.Page,
            Limit = paginacion.Limit,
            Offset = offset,
            Total = total
        };

        return RespuestaHttp.Ok(
            this,
            "Usuarios obtenidos correctamente.",
            resultado
        );
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        CrearUsuarioRequest request,
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

        // Validación de correo duplicado
        var correoExiste = await _db.Usuarios
            .AnyAsync(
                usuario => usuario.Correo == request.Correo,
                cancellationToken
            );

        if (correoExiste)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Ya existe un usuario con el correo indicado."
            );
        }

        // Validación de DNI duplicado
        var dniExiste = await _db.Usuarios
            .AnyAsync(
                usuario => usuario.Dni == request.Dni,
                cancellationToken
            );

        if (dniExiste)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Ya existe un usuario con el DNI indicado."
            );
        }

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Correo = request.Correo,
            Telefono = request.Telefono,
            Dni = request.Dni,
            Rol = request.Rol,
            Estado = EstadoUsuario.Pendiente
                .ToString()
                .ToLowerInvariant()
        };

        _db.Usuarios.Add(usuario);

        await _db.SaveChangesAsync(cancellationToken);

        var resultado = new
        {
            id = usuario.Id,
            nombre = usuario.Nombre,
            correo = usuario.Correo,
            telefono = usuario.Telefono,
            dni = usuario.Dni,
            rol = usuario.Rol,
            estado = usuario.Estado
        };

        return RespuestaHttp.Created(
            this,
            "Usuario creado correctamente.",
            resultado,
            nameof(ObtenerPorId),
            new { id = usuario.Id }
        );
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(
                usuario => usuario.Id == id,
                cancellationToken
            );

        if (usuario is null)
        {
            return RespuestaHttp.NotFound<object>(
                this,
                "El usuario no existe."
            );
        }

        var resultado = new
        {
            id = usuario.Id,
            nombre = usuario.Nombre,
            correo = usuario.Correo,
            telefono = usuario.Telefono,
            dni = usuario.Dni,
            rol = usuario.Rol,
            estado = usuario.Estado
        };

        return RespuestaHttp.Ok(
            this,
            "Usuario obtenido correctamente.",
            resultado
        );
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id,
        ActualizarUsuarioRequest request,
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

        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(
                usuario => usuario.Id == id,
                cancellationToken
            );

        if (usuario is null)
        {
            return RespuestaHttp.NotFound<object>(
                this,
                "El usuario no existe."
            );
        }

        // Validación del estado mediante la máquina de estados
        if (!Enum.TryParse<EstadoUsuario>(
                usuario.Estado,
                ignoreCase: true,
                out var estadoActual))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El usuario tiene un estado inválido."
            );
        }

        if (!ReglasEstado.Usuarios.TryGetValue(
                estadoActual,
                out var operacionesPermitidas) ||
            !operacionesPermitidas.Contains(OperacionEstado.Modificacion))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                $"No se puede modificar un usuario en estado '{usuario.Estado}'."
            );
        }

        // Validación de correo duplicado
        var correoExiste = await _db.Usuarios
            .AnyAsync(
                otro =>
                    otro.Id != id &&
                    otro.Correo == request.Correo,
                cancellationToken
            );

        if (correoExiste)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Ya existe otro usuario con el correo indicado."
            );
        }

        // Validación de DNI duplicado
        var dniExiste = await _db.Usuarios
            .AnyAsync(
                otro =>
                    otro.Id != id &&
                    otro.Dni == request.Dni,
                cancellationToken
            );

        if (dniExiste)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Ya existe otro usuario con el DNI indicado."
            );
        }

        usuario.Nombre = request.Nombre;
        usuario.Correo = request.Correo;
        usuario.Telefono = request.Telefono;
        usuario.Dni = request.Dni;
        usuario.Rol = request.Rol;

        await _db.SaveChangesAsync(cancellationToken);

        var resultado = new
        {
            id = usuario.Id,
            nombre = usuario.Nombre,
            correo = usuario.Correo,
            telefono = usuario.Telefono,
            dni = usuario.Dni,
            rol = usuario.Rol,
            estado = usuario.Estado
        };

        return RespuestaHttp.Ok(
            this,
            "Usuario actualizado correctamente.",
            resultado
        );
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(
        int id,
        CancellationToken cancellationToken)
    {
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(
                usuario => usuario.Id == id,
                cancellationToken
            );

        if (usuario is null)
        {
            return RespuestaHttp.NotFound<object>(
                this,
                "El usuario no existe."
            );
        }

        // Validación del estado mediante la máquina de estados
        if (!Enum.TryParse<EstadoUsuario>(
                usuario.Estado,
                ignoreCase: true,
                out var estadoActual))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El usuario tiene un estado inválido."
            );
        }

        if (!ReglasEstado.Usuarios.TryGetValue(
                estadoActual,
                out var operacionesPermitidas) ||
            !operacionesPermitidas.Contains(OperacionEstado.Baja))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                $"No se puede dar de baja un usuario en estado '{usuario.Estado}'."
            );
        }

        // Baja lógica
        usuario.Estado = EstadoUsuario.Bloqueado
            .ToString()
            .ToLowerInvariant();

        await _db.SaveChangesAsync(cancellationToken);

        var resultado = new
        {
            id = usuario.Id,
            estado = usuario.Estado
        };

        return RespuestaHttp.Ok(
            this,
            "Usuario dado de baja correctamente.",
            resultado
        );
    }
}