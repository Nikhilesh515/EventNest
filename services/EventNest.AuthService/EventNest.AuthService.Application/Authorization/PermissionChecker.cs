using EventNest.AuthService.Application.Interfaces;

namespace EventNest.AuthService.Application.Authorization;

public interface IPermissionChecker
{
    Task<bool> CheckAsync(Guid userId, string permissionName);
    Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(Guid userId);
}

public class PermissionChecker : IPermissionChecker
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionStore _permissionStore;

    public PermissionChecker(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionStore permissionStore)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionStore = permissionStore;
    }

    public async Task<bool> CheckAsync(Guid userId, string permissionName)
    {
        var permissions = await GetEffectivePermissionsAsync(userId);
        return permissions.Contains(permissionName);
    }

    public async Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return Array.Empty<string>();

        var role = await _roleRepository.GetByIdAsync(user.RoleId);
        if (role is null)
            return Array.Empty<string>();

        // Get role defaults
        var permissions = new List<string>();
        if (PermissionGroups.RoleDefaults.TryGetValue(role.Name, out var rolePerms))
        {
            permissions.AddRange(rolePerms);
        }

        // Admin and SuperAdmin get all permissions
        if (role.Name == "Admin" || role.Name == "SuperAdmin")
        {
            permissions = PermissionGroups.All.SelectMany(g => g.Value).Distinct().ToList();
        }

        // Get user-level overrides
        var userGrants = await _permissionStore.GetUserPermissionsAsync(userId);

        // For now, the PermissionStore already merges role defaults with user grants
        // So we just return what the PermissionStore gives us
        return userGrants;
    }
}
