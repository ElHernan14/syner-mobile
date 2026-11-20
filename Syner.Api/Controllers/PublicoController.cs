using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syner.Api.Data;

namespace Syner.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/publico")]
public sealed class PublicoController(SynerDbContext db) : ControllerBase
{
    [HttpGet("lotes")]
    public async Task<IActionResult> ObtenerLotes(CancellationToken cancellationToken)
    {
        var hoy = DateTime.UtcNow.Date;
        var lotes = await db.Lotes.AsNoTracking()
            .Where(x => x.Estado.ToLower() != "borrador" && x.Estado.ToLower() != "cancelado" && x.FechaFin >= hoy)
            .OrderByDescending(x => x.Id)
            .Select(x => new {
                x.Nombre, x.Descripcion, x.Categoria,
                precio_cupo = x.PrecioCupo, precio_mercado = x.PrecioMercado,
                imagen_avatar = x.ImagenAvatar,
                porcentaje_ahorro = x.PorcentajeAhorro,
                cupos_disponibles = x.CantidadCupos - x.CuposOcupados,
                fecha_fin = x.FechaFin,
                proveedor = x.Proveedor.Nombre,
                proveedor_verificado = x.Proveedor.Verificado
            }).ToListAsync(cancellationToken);
        return Ok(lotes);
    }

    [HttpGet("pedidos/{token}")]
    public async Task<IActionResult> ObtenerPedido(string token, CancellationToken cancellationToken)
    {
        if (token.Length != 32 || !token.All(Uri.IsHexDigit)) return NotFound();
        var pedido = await db.Pedidos.AsNoTracking()
            .Where(x => x.TokenPublico == token.ToLower())
            .Select(x => new {
                estado = x.Estado,
                lote = new {
                    x.Lote.Nombre, x.Lote.Categoria,
                    imagen_avatar = x.Lote.ImagenAvatar,
                    precio_cupo = x.Lote.PrecioCupo,
                    precio_mercado = x.Lote.PrecioMercado,
                    porcentaje_ahorro = x.Lote.PorcentajeAhorro,
                    proveedor = x.Lote.Proveedor.Nombre,
                    proveedor_verificado = x.Lote.Proveedor.Verificado
                }
            }).FirstOrDefaultAsync(cancellationToken);
        return pedido is null ? NotFound() : Ok(pedido);
    }
}
