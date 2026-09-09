using EventNest.Shared.Domain.Entities;

namespace EventNest.AuthService.Domain.Entities;

public class PermissionGrant : BaseEntity
{
    public string PermissionName { get; private set; } = string.Empty;
    public bool IsGranted { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public bool IsValid => !ExpiresAt.HasValue || ExpiresAt.Value > DateTime.UtcNow;

    private PermissionGrant() { }

    public static PermissionGrant Create(string permissionName, bool isGranted, Guid userId, DateTime? expiresAt = null)
    {
        return new PermissionGrant
        {
            Id = Guid.NewGuid(),
            PermissionName = permissionName,
            IsGranted = isGranted,
            ExpiresAt = expiresAt,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
