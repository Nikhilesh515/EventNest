using Microsoft.AspNetCore.Authorization;

namespace EventNest.AuthService.Application.Authorization;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new ArgumentException("Permission cannot be null or empty.", nameof(permission));
        Permission = permission;
    }
}
