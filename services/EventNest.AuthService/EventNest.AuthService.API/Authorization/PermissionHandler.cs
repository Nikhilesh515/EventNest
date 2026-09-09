using System.Security.Claims;
using EventNest.AuthService.Application.Interfaces;
using EventNest.Shared.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace EventNest.AuthService.API.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionStore _permissionStore;
    private readonly ILogger<PermissionHandler> _logger;

    public PermissionHandler(IPermissionStore permissionStore, ILogger<PermissionHandler> logger)
    {
        _permissionStore = permissionStore;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Authorization failed: Could not parse user identity from token.");
            return;
        }

        var permissions = await _permissionStore.GetUserPermissionsAsync(userId);
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning(
                "Authorization failed: User {UserId} lacks permission {Permission}",
                userId, requirement.Permission);
        }
    }
}
