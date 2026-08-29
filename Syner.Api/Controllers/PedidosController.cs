using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syner.Api.Data;

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
        CancellationToken cancellationToken)
    {
        var pedidos = await _db.Pedidos
            .AsNoTracking()
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
                    precio_cupo = pedido.Lote.PrecioCupo
                }
            })
            .ToListAsync(cancellationToken);

        return Ok(pedidos);
    }
}