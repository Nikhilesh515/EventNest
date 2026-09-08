using EventNest.AuthService.Application.Authorization;
using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Domain.Entities;
using EventNest.AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventNest.AuthService.Infrastructure.Services;

public class PermissionStore : IPermissionStore
{
    private readonly AuthDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICacheService _cacheService;

    public PermissionStore(
        AuthDbContext context,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICacheService cacheService)
    {
        _context = context;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _cacheService = cacheService;
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId)
    {
        var cacheKey = $"user:{userId}:permissions";
        var cached = await _cacheService.GetAsync<List<string>>(cacheKey);
        if (cached is not null)
            return cached;

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
        var grants = await _context.PermissionGrants
            .Where(g => g.UserId == userId && g.IsValid)
            .ToListAsync();

        foreach (var grant in grants)
        {
            if (grant.IsGranted && !permissions.Contains(grant.PermissionName))
                permissions.Add(grant.PermissionName);
            else if (!grant.IsGranted)
                permissions.Remove(grant.PermissionName);
        }

        // Cache result with 5 min TTL
        await _cacheService.SetAsync(cacheKey, permissions, TimeSpan.FromMinutes(5));

        return permissions;
    }

    public async Task AddGrantAsync(PermissionGrant grant)
    {
        await _context.PermissionGrants.AddAsync(grant);
        await _context.SaveChangesAsync();

        // Invalidate cache
        await _cacheService.RemoveAsync($"user:{grant.UserId}:permissions");
    }

    public async Task RemoveGrantAsync(Guid userId, string permissionName)
    {
        var grant = await _context.PermissionGrants
            .FirstOrDefaultAsync(g => g.UserId == userId && g.PermissionName == permissionName);

        if (grant is not null)
        {
            _context.PermissionGrants.Remove(grant);
            await _context.SaveChangesAsync();

            // Invalidate cache
            await _cacheService.RemoveAsync($"user:{userId}:permissions");
        }
    }

    public async Task<PermissionGrant?> GetGrantAsync(Guid userId, string permissionName)
    {
        return await _context.PermissionGrants
            .FirstOrDefaultAsync(g => g.UserId == userId && g.PermissionName == permissionName);
    }
}
