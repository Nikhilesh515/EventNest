using EventNest.AuthService.Application.Interfaces;

namespace EventNest.AuthService.Application.Authorization;

public interface IPermissionChecker
{
    Task<bool> CheckAsync(Guid userId, string permissionName);
    Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(Guid userId);
}

public class PermissionChecker : IPermissionChecker
{
    private readonly IPermissionStore _permissionStore;

    public PermissionChecker(IPermissionStore permissionStore)
    {
        _permissionStore = permissionStore;
    }

    public async Task<bool> CheckAsync(Guid userId, string permissionName)
    {
        var permissions = await GetEffectivePermissionsAsync(userId);
        return permissions.Contains(permissionName);
    }

    public Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(Guid userId)
    {
        return _permissionStore.GetUserPermissionsAsync(userId);
    }
}
