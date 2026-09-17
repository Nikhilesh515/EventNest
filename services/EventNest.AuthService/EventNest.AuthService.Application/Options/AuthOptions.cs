namespace EventNest.AuthService.Application.Options;

public class AuthOptions
{
    public int RefreshRotationGraceSeconds { get; set; } = 30;
    public bool CookieSecure { get; set; } = true;
    public string[] AllowedOrigins { get; set; } = ["http://localhost:5173", "https://localhost:5173"];
}
