using Microsoft.AspNetCore.Mvc;

namespace Syner.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult ObtenerEstado()
    {
        return Ok(new
        {
            estado = "ok",
            mensaje = "API SYNER funcionando correctamente."
        });
    }
}