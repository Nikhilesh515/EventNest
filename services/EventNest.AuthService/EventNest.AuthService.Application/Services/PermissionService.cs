using EventNest.Shared.Application.Authorization;
using EventNest.AuthService.Application.DTOs.Permissions;
using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Application.Services.Interfaces;
using EventNest.AuthService.Domain.Entities;
using EventNest.Shared.Domain.Exceptions;

namespace EventNest.AuthService.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionStore _permissionStore;
    private readonly IUserRepository _userRepository;

    public PermissionService(IPermissionStore permissionStore, IUserRepository userRepository)
    {
        _permissionStore = permissionStore;
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<PermissionDto>> GetUserPermissionsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException($"User with ID '{userId}' was not found.");

        var permissionNames = await _permissionStore.GetUserPermissionsAsync(userId);
        var result = new List<PermissionDto>();

        foreach (var permissionName in permissionNames)
        {
            var (group, displayName) = GetPermissionInfo(permissionName);
            result.Add(new PermissionDto(permissionName, displayName, group, true));
        }

        return result;
    }

    public async Task<PermissionDto> GrantAsync(Guid userId, string permissionName, DateTime? expiresAt)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException($"User with ID '{userId}' was not found.");

        if (!IsValidPermission(permissionName))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["permissionName"] = new[] { $"Permission '{permissionName}' is not valid." }
            });

        var existingGrant = await _permissionStore.GetGrantAsync(userId, permissionName);
        if (existingGrant is not null)
            throw new ConflictException($"Permission '{permissionName}' is already granted to user.");

        var grant = PermissionGrant.Create(permissionName, true, userId, expiresAt);
        await _permissionStore.AddGrantAsync(grant);

        var (group, displayName) = GetPermissionInfo(permissionName);
        return new PermissionDto(permissionName, displayName, group, true);
    }

    public async Task RevokeAsync(Guid userId, string permissionName)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException($"User with ID '{userId}' was not found.");

        var existingGrant = await _permissionStore.GetGrantAsync(userId, permissionName);
        if (existingGrant is null)
            throw new NotFoundException($"Permission '{permissionName}' is not granted to user.");

        await _permissionStore.RemoveGrantAsync(userId, permissionName);
    }

    public async Task<bool> CheckAsync(Guid userId, string permissionName)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return false;

        var permissions = await _permissionStore.GetUserPermissionsAsync(userId);
        return permissions.Contains(permissionName);
    }

    private static bool IsValidPermission(string permissionName)
    {
        return EventNestPermissions.Events.View == permissionName ||
               EventNestPermissions.Events.Create == permissionName ||
               EventNestPermissions.Events.Edit == permissionName ||
               EventNestPermissions.Events.Delete == permissionName ||
               EventNestPermissions.Tags.View == permissionName ||
               EventNestPermissions.Tags.Create == permissionName ||
               EventNestPermissions.Tags.Edit == permissionName ||
               EventNestPermissions.Tags.Delete == permissionName ||
               EventNestPermissions.RSVPs.View == permissionName ||
               EventNestPermissions.RSVPs.Create == permissionName ||
               EventNestPermissions.RSVPs.Manage == permissionName ||
               EventNestPermissions.RSVPs.Cancel == permissionName ||
               EventNestPermissions.Users.View == permissionName ||
               EventNestPermissions.Users.Manage == permissionName;
    }

    private static (string Group, string DisplayName) GetPermissionInfo(string permissionName)
    {
        return permissionName switch
        {
            "Events.View" => ("Events", "View Events"),
            "Events.Create" => ("Events", "Create Events"),
            "Events.Edit" => ("Events", "Edit Events"),
            "Events.Delete" => ("Events", "Delete Events"),
            "Tags.View" => ("Tags", "View Tags"),
            "Tags.Create" => ("Tags", "Create Tags"),
            "Tags.Edit" => ("Tags", "Edit Tags"),
            "Tags.Delete" => ("Tags", "Delete Tags"),
            "RSVPs.View" => ("RSVPs", "View RSVPs"),
            "RSVPs.Create" => ("RSVPs", "Create RSVPs"),
            "RSVPs.Manage" => ("RSVPs", "Manage RSVPs"),
            "RSVPs.Cancel" => ("RSVPs", "Cancel RSVPs"),
            "Users.View" => ("Users", "View Users"),
            "Users.Manage" => ("Users", "Manage Users"),
            _ => ("Unknown", permissionName)
        };
    }
}
