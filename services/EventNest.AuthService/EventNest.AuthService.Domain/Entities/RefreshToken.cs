namespace EventNest.AuthService.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string CreatedByIp { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    public static RefreshToken Create(string token, DateTime expiresAt, string createdByIp, Guid userId)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Revoke(string? revokedByIp)
    {
        IsRevoked = true;
        RevokedByIp = revokedByIp;
    }
}
