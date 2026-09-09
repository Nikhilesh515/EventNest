using System.Security.Claims;
using EventNest.TagService.Application.Interfaces;
using EventNest.Shared.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace EventNest.TagService.API.Authorization;

public class TagPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<TagPermissionHandler> _logger;

    public TagPermissionHandler(ICacheService cacheService, ILogger<TagPermissionHandler> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
        {
            _logger.LogWarning("Authorization failed: No user identity found in token.");
            return;
        }

        var cacheKey = $"user:{userIdClaim}:permissions";
        var permissions = await _cacheService.GetAsync<List<string>>(cacheKey);

        if (permissions is null)
        {
            _logger.LogWarning(
                "Permissions not cached for user {UserId}. Ensure AuthService has populated the cache.",
                userIdClaim);
            return;
        }

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
    }
}
