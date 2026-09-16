namespace EventNest.Shared.Infrastructure.Configuration;

public static class JwtConfigurationGuard
{
    private const string PlaceholderSecret = "YOUR_SECRET_KEY_HERE_MIN_32_CHARS_LONG!!";

    public static void Validate(string? secretKey, bool isDevelopment)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
            throw new InvalidOperationException(
                "Jwt:SecretKey is not configured. Provide it via the Jwt__SecretKey environment variable or another configuration source.");

        if (secretKey.Length < 32)
            throw new InvalidOperationException(
                "Jwt:SecretKey must be at least 32 characters long.");

        if (!isDevelopment && secretKey == PlaceholderSecret)
            throw new InvalidOperationException(
                "Jwt:SecretKey is still the placeholder value. Configure a real secret (JWT_SECRET_KEY / Jwt__SecretKey) outside Development.");
    }
}
