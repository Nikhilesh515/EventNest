using System.Security.Claims;
using EventNest.RSVPService.Application.Interfaces;
using EventNest.Shared.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace EventNest.RSVPService.API.Authorization;

public class RsvpPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<RsvpPermissionHandler> _logger;

    public RsvpPermissionHandler(ICacheService cacheService, ILogger<RsvpPermissionHandler> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return;

        var permissions = await _cacheService.GetAsync<List<string>>($"user:{userId}:permissions");
        if (permissions is null)
        {
            _logger.LogWarning("Permissions not cached for user {UserId}.", userId);
            return;
        }

        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
    }
}
