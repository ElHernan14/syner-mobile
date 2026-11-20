using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Syner.Api.Controllers;

[AllowAnonymous]
public sealed class PaginasPublicasController : Controller
{
    [HttpGet("/.well-known/assetlinks.json")]
    public IActionResult AssetLinks([FromServices] IConfiguration configuration)
    {
        var fingerprints = configuration["AndroidAppLinks:Sha256CertFingerprints"]?
            .Split(
                new[] { ',', ';' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            )
            ?? [];

        if (fingerprints.Length == 0 ||
            fingerprints.Any(fingerprint =>
                !Regex.IsMatch(
                    fingerprint,
                    "^([0-9A-Fa-f]{2}:){31}[0-9A-Fa-f]{2}$"
                )))
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "Android App Links todavía no están configurados."
            );
        }

        return new JsonResult(new[]
        {
            new
            {
                relation = new[] { "delegate_permission/common.handle_all_urls" },
                target = new Dictionary<string, object>
                {
                    ["namespace"] = "android_app",
                    ["package_name"] = "com.syner.app",
                    ["sha256_cert_fingerprints"] = fingerprints
                }
            }
        });
    }

    [HttpGet("/catalogo")]
    public IActionResult Catalogo()
    {
        const string pagina = """
            <!doctype html>
            <html lang="es">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <meta name="theme-color" content="#123a78">
              <title>Catálogo público · SYNER</title>
              <style>
                *{box-sizing:border-box}
                body{margin:0;background:#f4f7fb;color:#172033;font:16px system-ui,-apple-system,Segoe UI,sans-serif}
                main{max-width:1080px;margin:auto;padding:28px 20px 48px}
                header{padding:32px;border-radius:24px;background:linear-gradient(135deg,#123a78,#2877d4);color:#fff}
                header small{letter-spacing:.14em}h1{font-size:clamp(28px,6vw,46px);margin:10px 0}
                header p{margin:0;color:#e5efff}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(min(100%,280px),1fr));gap:16px;margin:24px 0}
                article{display:flex;flex-direction:column;gap:10px;padding:20px;border:1px solid #e3eaf3;border-radius:20px;background:#fff;box-shadow:0 12px 32px #15335a13}
                article small,footer{color:#66758c}article h2,article p{margin:0}article h2{font-size:21px}
                article strong{font-size:24px}article s{color:#78859a}.verified,.saving{color:#14845d;font-weight:700}
                footer{margin-top:auto;font-size:13px}.privacy{color:#2469bd}
                .message{padding:18px;border-radius:14px;background:#fff;color:#66758c}
              </style>
            </head>
            <body>
              <main>
                <header><small>SYNER · CATÁLOGO PÚBLICO</small><h1>Encontrá tu próximo cupo</h1><p>Lotes disponibles de proveedores de la comunidad.</p></header>
                <p id="message" class="message">Cargando catálogo…</p>
                <section id="lotes" class="grid" aria-live="polite"></section>
                <a class="privacy" href="/privacidad.html">Política de privacidad</a>
              </main>
              <script>
                const moneda = valor => new Intl.NumberFormat("es-AR",{style:"currency",currency:"ARS",maximumFractionDigits:0}).format(valor);
                const mensaje = document.querySelector("#message");
                fetch("/api/publico/lotes").then(respuesta => {
                  if (!respuesta.ok) throw new Error("No pudimos cargar el catálogo. Probá nuevamente más tarde.");
                  return respuesta.json();
                }).then(lotes => {
                  mensaje.hidden = lotes.length > 0;
                  if (!lotes.length) { mensaje.textContent = "Todavía no hay lotes disponibles."; return; }
                  for (const lote of lotes) {
                    const tarjeta = document.createElement("article");
                    const categoria = document.createElement("small");
                    categoria.textContent = lote.categoria + " · " + lote.proveedor + (lote.proveedor_verificado ? " · ✓ Verificado" : "");
                    if (lote.proveedor_verificado) categoria.className = "verified";
                    if (lote.imagen_avatar) {
                      let urlImagen = null;
                      try {
                        const url = new URL(lote.imagen_avatar, window.location.origin);
                        if (url.protocol === "https:" || url.origin === window.location.origin) urlImagen = url;
                      } catch (error) {
                        console.warn("Se omitió una URL de imagen de lote no válida.", error);
                      }
                      if (urlImagen) {
                        const imagen = document.createElement("img");
                        imagen.src = urlImagen.href;
                        imagen.alt = "Imagen del lote " + lote.nombre;
                        imagen.loading = "lazy";
                        imagen.style.cssText = "width:calc(100% + 40px);height:190px;margin:-20px -20px 4px;object-fit:cover;border-radius:20px 20px 0 0;background:#e9eef5";
                        tarjeta.append(imagen);
                      }
                    }
                    const nombre = document.createElement("h2"); nombre.textContent = lote.nombre;
                    const descripcion = document.createElement("p"); descripcion.textContent = lote.descripcion;
                    const precio = document.createElement("strong"); precio.textContent = moneda(lote.precio_cupo);
                    const mercado = document.createElement("s"); mercado.textContent = moneda(lote.precio_mercado);
                    const ahorro = document.createElement("span"); ahorro.className = "saving"; ahorro.textContent = lote.porcentaje_ahorro + "% de ahorro";
                    const pie = document.createElement("footer");
                    pie.textContent = lote.cupos_disponibles + " cupos · Finaliza " + new Date(lote.fecha_fin).toLocaleDateString("es-AR");
                    tarjeta.append(categoria,nombre,descripcion,precio,mercado,ahorro,pie);
                    document.querySelector("#lotes").append(tarjeta);
                  }
                }).catch(error => { mensaje.textContent = error.message; });
              </script>
            </body>
            </html>
            """;

        return Content(pagina, "text/html; charset=utf-8");
    }

    [HttpGet("/p/{token}")]
    public IActionResult Pedido(string token)
    {
        if (token.Length != 32 || !token.All(Uri.IsHexDigit)) return NotFound();
        const string plantilla = """<!doctype html><html lang="es"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta name="theme-color" content="#123a78"><title>Registro público SYNER</title><style>body{font:16px system-ui;background:#f4f7fb;color:#172033;margin:0;padding:24px}main{max-width:680px;margin:8vh auto;background:white;padding:28px;border-radius:24px;box-shadow:0 12px 32px #15335a18}h1{font-size:32px}small{color:#66758c}.pill{display:inline-block;background:#e7f4ef;color:#147854;padding:8px 12px;border-radius:30px}#error{color:#aa3040}</style><main><small>SYNER · REGISTRO PÚBLICO</small><h1 id="nombre">Cargando…</h1><p id="estado"></p><p id="lote"></p><h2 id="precio"></h2><p id="extra"></p><small>Esta página no muestra datos personales del solicitante.</small><p id="error"></p></main><script>fetch('/api/publico/pedidos/__TOKEN__').then(r=>{if(!r.ok)throw Error();return r.json()}).then(p=>{document.querySelector('#nombre').textContent=p.lote.nombre;document.querySelector('#estado').innerHTML='<span class="pill"></span>';document.querySelector('.pill').textContent=p.estado;document.querySelector('#lote').textContent=p.lote.categoria+' · '+p.lote.proveedor+(p.lote.proveedor_verificado?' ✓ Verificado':'');document.querySelector('#precio').textContent=new Intl.NumberFormat('es-AR',{style:'currency',currency:'ARS'}).format(p.lote.precio_cupo);document.querySelector('#extra').textContent='Precio de mercado: '+new Intl.NumberFormat('es-AR',{style:'currency',currency:'ARS'}).format(p.lote.precio_mercado)+' · Ahorro '+p.lote.porcentaje_ahorro+'%'}).catch(()=>{document.querySelector('#nombre').textContent='Registro no disponible';document.querySelector('#error').textContent='El enlace puede haber vencido o no existir.'})}</script></html>""";
        return Content(plantilla.Replace("__TOKEN__", Uri.EscapeDataString(token)), "text/html; charset=utf-8");
    }

    [HttpGet("/privacidad.html")]
    public IActionResult Privacidad()
    {
        const string pagina = """
            <!doctype html>
            <html lang="es">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width,initial-scale=1">
              <meta name="theme-color" content="#123a78">
              <title>Privacidad · SYNER</title>
              <style>
                body{font:16px system-ui,-apple-system,Segoe UI,sans-serif;background:#f4f7fb;color:#172033;margin:0;padding:24px}
                main{max-width:760px;margin:6vh auto;padding:28px;border-radius:22px;background:#fff;box-shadow:0 12px 32px #15335a13;line-height:1.65}
                h1{line-height:1.2}a{color:#2469bd}
              </style>
            </head>
            <body>
              <main>
                <h1>Política de privacidad de SYNER</h1>
                <p><strong>Responsable:</strong> Hernán Gonzalo Constante.</p>
                <p>SYNER utiliza los datos de cuenta para autenticar usuarios, gestionar lotes y pedidos, y enviar notificaciones operativas. El acceso público por enlace muestra únicamente datos del lote y el estado del registro; nunca muestra el nombre, correo, teléfono ni identificador del solicitante.</p>
                <p>Las credenciales se almacenan de forma protegida y el inicio con Google solo habilita cuentas previamente registradas y verificadas por SYNER.</p>
                <p>Para solicitar acceso, corrección o eliminación de datos, escribí a <a href="mailto:hernanbonne98@gmail.com">hernanbonne98@gmail.com</a>.</p>
              </main>
            </body>
            </html>
            """;

        return Content(pagina, "text/html; charset=utf-8");
    }
}
