using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syner.Api.Data;

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
        CancellationToken cancellationToken)
    {
        var usuarios = await _db.Usuarios
            .AsNoTracking()
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

        return Ok(usuarios);
    }
}   