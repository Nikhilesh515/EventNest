using System.Security.Claims;
using EventNest.Shared.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace EventNest.TagService.API.Authorization;

public class TagPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<TagPermissionHandler> _logger;

    public TagPermissionHandler(ILogger<TagPermissionHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
        {
            _logger.LogWarning("Authorization failed: No user identity found in token.");
            return Task.CompletedTask;
        }

        var permissions = context.User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList();

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning(
                "Authorization failed: User {UserId} lacks permission {Permission}",
                userIdClaim, requirement.Permission);
        }

        return Task.CompletedTask;
    }
}
