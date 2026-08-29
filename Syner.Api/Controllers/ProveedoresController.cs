using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syner.Api.Data;

namespace Syner.Api.Controllers;

[ApiController]
[Route("api/proveedores")]
public sealed class ProveedoresController : ControllerBase
{
    private readonly SynerDbContext _db;

    public ProveedoresController(SynerDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        CancellationToken cancellationToken)
    {
        var proveedores = await _db.Proveedores
            .AsNoTracking()
            .Select(proveedor => new
            {
                id = proveedor.Id,
                nombre = proveedor.Nombre,
                descripcion = proveedor.Descripcion,
                verificado = proveedor.Verificado
            })
            .ToListAsync(cancellationToken);

        return Ok(proveedores);
    }
}