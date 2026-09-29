using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Syner.Api.Data;
using Syner.Api.Data.Responses;
using Syner.Api.Data.Validation;
using Syner.Api.Domain.DTOs.Usuarios;
using Syner.Api.Domain.Entities;
using Syner.Api.Domain.Enums;
using Syner.Api.Domain.States;
using Syner.Api.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;


namespace Syner.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/usuarios")]
public sealed class UsuariosController : ControllerBase
{
    private readonly SynerDbContext _db;
    private readonly PasswordService _passwordService;
    private readonly IWebHostEnvironment _environment;

    public UsuariosController(
        SynerDbContext db,
        PasswordService passwordService,
        IWebHostEnvironment environment)
    {
        _environment = environment;
        _db = db;
        _passwordService = passwordService;
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
            .Include(usuario => usuario.Rol)
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
                consulta.OrderBy(usuario => usuario.Rol!.Nombre),

            "rol_desc" =>
                consulta.OrderByDescending(usuario => usuario.Rol!.Nombre),

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
                rol = usuario.Rol != null
                    ? usuario.Rol.Nombre
                    : null,
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
    [Authorize(Roles = "admin")]
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

        var rol = await _db.Roles
            .FirstOrDefaultAsync(
                x => x.Id == request.RolId,
                cancellationToken
            );

        if (rol is null)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El rol indicado no existe."
            );
        }

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Correo = request.Correo,
            Telefono = request.Telefono,
            Dni = request.Dni,
            RolId = rol.Id,
            PasswordHash = _passwordService.Hash(request.Password),
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
            rol = rol.Nombre,
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
            .Include(x => x.Rol)
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
            rol = usuario.Rol?.Nombre,
            estado = usuario.Estado
        };

        return RespuestaHttp.Ok(
            this,
            "Usuario obtenido correctamente.",
            resultado
        );
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
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
            .Include(x => x.Rol)
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

        var rol = await _db.Roles
            .FirstOrDefaultAsync(
                x => x.Id == request.RolId,
                cancellationToken
            );

        if (rol is null)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El rol indicado no existe."
            );
        }

        usuario.Nombre = request.Nombre;
        usuario.Correo = request.Correo;
        usuario.Telefono = request.Telefono;
        usuario.Dni = request.Dni;
        usuario.RolId = rol.Id;

        // La contraseña solamente se modifica si se envía una nueva.
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            usuario.PasswordHash = _passwordService.Hash(
                request.Password
            );
        }

        await _db.SaveChangesAsync(cancellationToken);

        var resultado = new
        {
            id = usuario.Id,
            nombre = usuario.Nombre,
            correo = usuario.Correo,
            telefono = usuario.Telefono,
            dni = usuario.Dni,
            rol = rol.Nombre,
            estado = usuario.Estado
        };

        return RespuestaHttp.Ok(
            this,
            "Usuario actualizado correctamente.",
            resultado
        );
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
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

    [HttpPost("perfil/avatar")]
    public async Task<IActionResult> ActualizarAvatar(
        IFormFile avatar)
    {
        var usuario_id_claim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!long.TryParse(usuario_id_claim, out var usuario_id))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "No se pudo identificar al usuario autenticado.",
                "El token no contiene un identificador de usuario válido."
            );
        }

        if (avatar is null || avatar.Length == 0)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "No se recibió ninguna imagen.",
                "El archivo es obligatorio."
            );
        }

        var extensiones_permitidas = new[]
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        var extension =
            Path.GetExtension(avatar.FileName)
                .ToLowerInvariant();

        if (!extensiones_permitidas.Contains(extension))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Formato de imagen no válido.",
                "Solo se permiten imágenes JPG, JPEG, PNG o WEBP."
            );
        }

        const long tamaño_maximo = 5 * 1024 * 1024;

        if (avatar.Length > tamaño_maximo)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "La imagen supera el tamaño máximo permitido.",
                "El tamaño máximo permitido es de 2 MB."
            );
        }

        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuario_id);

        if (usuario is null)
        {
            return RespuestaHttp.NotFound<object>(
                this,
                "Usuario no encontrado.",
                "No existe el usuario autenticado."
            );
        }

        if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "No se configuró el directorio público de la aplicación.",
                "No se encontró el directorio wwwroot."
            );
        }

        var carpeta = Path.Combine(
            _environment.WebRootPath,
            "uploads",
            "usuarios"
        );

        Directory.CreateDirectory(carpeta);

        var nombre_archivo =
            $"{Guid.NewGuid()}{extension}";

        var ruta_archivo =
            Path.Combine(carpeta, nombre_archivo);

        await using (var stream = new FileStream(
            ruta_archivo,
            FileMode.Create))
        {
            await avatar.CopyToAsync(stream);
        }

        usuario.Avatar =
            $"/uploads/usuarios/{nombre_archivo}";

        await _db.SaveChangesAsync();

        return RespuestaHttp.Ok(
            this,
            "Foto de perfil actualizada correctamente.",
            new
            {
                avatar = usuario.Avatar
            }
        );
    }
}