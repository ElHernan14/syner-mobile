using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Extensions.Options;

using Microsoft.IdentityModel.Tokens;

using Syner.Api.Configuration;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Services;

public sealed class JwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerarAccessToken(Usuario usuario)
    {
        if (string.IsNullOrWhiteSpace(_options.Key))
        {
            throw new InvalidOperationException(
                "La clave de firma JWT no está configurada."
            );
        }

        if (string.IsNullOrWhiteSpace(_options.Issuer))
        {
            throw new InvalidOperationException(
                "El issuer JWT no está configurado."
            );
        }

        if (string.IsNullOrWhiteSpace(_options.Audience))
        {
            throw new InvalidOperationException(
                "El audience JWT no está configurado."
            );
        }

        if (_options.ExpirationMinutes <= 0)
        {
            throw new InvalidOperationException(
                "La duración del token JWT debe ser mayor a cero."
            );
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Correo),
            new(ClaimTypes.Name, usuario.Nombre)
        };

        if (usuario.Rol is not null)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    usuario.Rol.Nombre
                )
            );
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.Key)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var expiration = DateTime.UtcNow.AddMinutes(
            _options.ExpirationMinutes
        );

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}