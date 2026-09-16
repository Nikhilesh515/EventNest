using EventNest.Shared.Domain.Entities;

namespace EventNest.AuthService.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public bool IsActive { get; private set; }

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private readonly List<PermissionGrant> _permissionGrants = new();
    public IReadOnlyCollection<PermissionGrant> PermissionGrants => _permissionGrants.AsReadOnly();

    private User() { }

    public static User Create(string email, string displayName, string passwordHash, Guid roleId, Guid? id = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Email = email,
            DisplayName = displayName,
            PasswordHash = passwordHash,
            RoleId = roleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDisplayName(string displayName)
    {
        DisplayName = displayName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(Guid roleId)
    {
        RoleId = roleId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
