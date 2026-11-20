using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Syner.Api.Configuration;
using Syner.Api.Data;
using Syner.Api.Data.Responses;
using Syner.Api.Data.Validation;
using Syner.Api.Domain.DTOs.Auth;
using Syner.Api.Services;
using Syner.Api.Domain.Entities;
using Google.Apis.Auth;
using System.Text.Json.Serialization;

namespace Syner.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly SynerDbContext _db;
    private readonly PasswordService _passwordService;
    private readonly JwtTokenService _jwtTokenService;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly JwtOptions _jwtOptions;
    private readonly IConfiguration _configuration;

    public AuthController(
        SynerDbContext db,
        PasswordService passwordService,
        JwtTokenService jwtTokenService,
        RefreshTokenService refreshTokenService,
        IOptions<JwtOptions> jwtOptions,
        IConfiguration configuration)
    {
        _db = db;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _jwtOptions = jwtOptions.Value;
        _configuration = configuration;
    }

    public sealed record GoogleLoginRequest([property: JsonPropertyName("id_token")] string IdToken);

    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginGoogle(GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        var webClientId = _configuration["GoogleAuth:WebClientId"];
        if (string.IsNullOrWhiteSpace(webClientId))
            return StatusCode(503, new { exito = false, mensaje = "El ingreso con Google aún no está configurado." });
        if (string.IsNullOrWhiteSpace(request.IdToken)) return BadRequest(new { exito = false, mensaje = "Google no devolvió un token válido." });

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { webClientId }
            });
        }
        catch (InvalidJwtException)
        {
            return Unauthorized(new { exito = false, mensaje = "La credencial de Google no es válida o expiró." });
        }

        if (payload.Email is null || !payload.EmailVerified)
            return Unauthorized(new { exito = false, mensaje = "La cuenta de Google no tiene un correo verificado." });

        var correo = payload.Email.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.Include(x => x.Rol).FirstOrDefaultAsync(x => x.Correo.ToLower() == correo, cancellationToken);
        if (usuario is null || !string.Equals(usuario.Estado, "verificado", StringComparison.OrdinalIgnoreCase))
            return StatusCode(403, new { exito = false, mensaje = "Tu cuenta de Google no está habilitada en SYNER. Solicitá el alta a un administrador." });

        await _refreshTokenService.LimpiarTokensInutilizables(_db, usuario.Id, cancellationToken);
        var accessToken = _jwtTokenService.GenerarAccessToken(usuario);
        var refreshToken = _refreshTokenService.GenerarToken();
        _db.RefreshTokens.Add(_refreshTokenService.Crear(usuario, refreshToken, _jwtOptions.RefreshTokenExpirationDays));
        await _db.SaveChangesAsync(cancellationToken);
        return RespuestaHttp.Ok(this, "Inicio de sesión con Google correcto.", new
        {
            accessToken, refreshToken, expiresIn = _jwtOptions.ExpirationMinutes * 60,
            usuario = new { id = usuario.Id, nombre = usuario.Nombre, correo = usuario.Correo, rol = usuario.Rol?.Nombre, estado = usuario.Estado }
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        LoginRequest request,
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
            .Include(x => x.Rol)
            .FirstOrDefaultAsync(
                x => x.Correo == request.Correo,
                cancellationToken
            );

        if (usuario is null)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El correo o la contraseña son incorrectos."
            );
        }

        if (string.IsNullOrWhiteSpace(usuario.PasswordHash))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El usuario no tiene una contraseña configurada."
            );
        }

        var passwordCorrecta = _passwordService.Verify(
            request.Password,
            usuario.PasswordHash
        );

        if (!passwordCorrecta)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El correo o la contraseña son incorrectos."
            );
        }

        if (!string.Equals(
                usuario.Estado,
                "verificado",
                StringComparison.OrdinalIgnoreCase))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El usuario no se encuentra habilitado para iniciar sesión."
            );
        }

        await _refreshTokenService.LimpiarTokensInutilizables(
            _db,
            usuario.Id,
            cancellationToken
        );

        var accessToken = _jwtTokenService.GenerarAccessToken(
            usuario
        );

        var refreshToken = _refreshTokenService.GenerarToken();

        var refreshTokenEntity = _refreshTokenService.Crear(
            usuario,
            refreshToken,
            _jwtOptions.RefreshTokenExpirationDays
        );

        _db.RefreshTokens.Add(refreshTokenEntity);

        await _db.SaveChangesAsync(cancellationToken);

        var resultado = new
        {
            accessToken,
            refreshToken,
            expiresIn = _jwtOptions.ExpirationMinutes * 60,
            usuario = new
            {
                id = usuario.Id,
                nombre = usuario.Nombre,
                correo = usuario.Correo,
                rol = usuario.Rol?.Nombre,
                estado = usuario.Estado
            }
        };

        return RespuestaHttp.Ok(
            this,
            "Inicio de sesión correcto.",
            resultado
        );
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        RefreshTokenRequest request,
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

        var tokenHash = _refreshTokenService.GenerarHash(
            request.RefreshToken
        );

        var refreshToken = await _db.RefreshTokens
            .Include(x => x.Usuario)
            .ThenInclude(x => x.Rol)
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken
            );

        if (refreshToken is null)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El refresh token no es válido."
            );
        }

        if (refreshToken.EstaRevocado)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El refresh token ya fue revocado."
            );
        }

        if (refreshToken.EstaExpirado)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El refresh token ha expirado."
            );
        }

        var usuario = refreshToken.Usuario;

        if (!string.Equals(
                usuario.Estado,
                "verificado",
                StringComparison.OrdinalIgnoreCase))
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "El usuario no se encuentra habilitado para renovar la sesión."
            );
        }

        await _refreshTokenService.LimpiarTokensInutilizables(
            _db,
            usuario.Id,
            cancellationToken
        );

        refreshToken.RevocadoEn = DateTime.UtcNow;

        var accessToken = _jwtTokenService.GenerarAccessToken(
            usuario
        );

        var nuevoRefreshToken = _refreshTokenService.GenerarToken();

        var nuevoRefreshTokenEntity = _refreshTokenService.Crear(
            usuario,
            nuevoRefreshToken,
            _jwtOptions.RefreshTokenExpirationDays
        );

        _db.RefreshTokens.Add(nuevoRefreshTokenEntity);

        await _db.SaveChangesAsync(cancellationToken);

        var resultado = new
        {
            accessToken,
            refreshToken = nuevoRefreshToken,
            expiresIn = _jwtOptions.ExpirationMinutes * 60,
            usuario = new
            {
                id = usuario.Id,
                nombre = usuario.Nombre,
                correo = usuario.Correo,
                rol = usuario.Rol?.Nombre,
                estado = usuario.Estado
            }
        };

        return RespuestaHttp.Ok(
            this,
            "Sesión renovada correctamente.",
            resultado
        );
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(
        RefreshTokenRequest request,
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

        var tokenHash = _refreshTokenService.GenerarHash(
            request.RefreshToken
        );

        var refreshToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken
            );

        if (refreshToken is not null &&
            !refreshToken.RevocadoEn.HasValue)
        {
            refreshToken.RevocadoEn = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
        }

        return RespuestaHttp.Ok(
            this,
            "Sesión cerrada correctamente.",
            "Puede cerrar la sesión en el cliente eliminando el access token y el refresh token almacenados."
        );
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        RegistrarUsuarioRequest request,
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

        var correo = request.Correo.Trim().ToLowerInvariant();
        var dni = request.Dni.Trim();

        var correoExiste = await _db.Usuarios
            .AnyAsync(x => x.Correo == correo, cancellationToken);

        if (correoExiste)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Ya existe un usuario registrado con ese correo."
            );
        }

        var dniExiste = await _db.Usuarios
            .AnyAsync(x => x.Dni == dni, cancellationToken);

        if (dniExiste)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "Ya existe un usuario registrado con ese DNI."
            );
        }

        var rolUsuario = await _db.Roles
            .SingleOrDefaultAsync(
                x => x.Nombre == "usuario",
                cancellationToken
            );

        if (rolUsuario is null)
        {
            return RespuestaHttp.BadRequest<object>(
                this,
                "No se encuentra configurado el rol de usuario."
            );
        }

        var usuario = new Usuario
        {
            Nombre = request.Nombre.Trim(),
            Correo = correo,
            Telefono = request.Telefono.Trim(),
            Dni = dni,
            RolId = rolUsuario.Id,
            PasswordHash = _passwordService.Hash(request.Password),
            Estado = "verificado"
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
            rol = rolUsuario.Nombre,
            estado = usuario.Estado
        };

        return RespuestaHttp.Ok(
            this,
            "Usuario registrado correctamente.",
            resultado
        );
    }

}
