using EventNest.AuthService.Application.Interfaces;
using BCryptNet = BCrypt.Net.BCrypt;

namespace EventNest.AuthService.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        return BCryptNet.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string hash)
    {
        return BCryptNet.Verify(password, hash);
    }
}
