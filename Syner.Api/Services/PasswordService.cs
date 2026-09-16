using Microsoft.AspNetCore.Identity;

namespace Syner.Api.Services;

public sealed class PasswordService
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
    {
        return _hasher.HashPassword(
            new object(),
            password
        );
    }

    public bool Verify(
        string password,
        string passwordHash)
    {
        var resultado = _hasher.VerifyHashedPassword(
            new object(),
            passwordHash,
            password
        );

        return resultado == PasswordVerificationResult.Success ||
               resultado == PasswordVerificationResult.SuccessRehashNeeded;
    }
}