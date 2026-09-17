using System.Security.Cryptography;
using System.Text;
using EventNest.AuthService.Application.Interfaces;

namespace EventNest.AuthService.Infrastructure.Services;

public class TokenHasher : ITokenHasher
{
    public string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
