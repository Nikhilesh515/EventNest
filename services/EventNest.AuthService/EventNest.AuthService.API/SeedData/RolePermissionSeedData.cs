using EventNest.AuthService.Domain.Entities;
using EventNest.AuthService.Infrastructure.Data;
using EventNest.Shared.Application.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EventNest.AuthService.API.SeedData;

public static class RolePermissionSeedData
{
    public static async Task SeedAsync(AuthDbContext context)
    {
        if (await context.RolePermissions.AnyAsync())
            return;

        var roles = await context.Roles.ToListAsync();
        if (roles.Count == 0)
            return;

        var entries = new List<RolePermission>();

        foreach (var role in roles)
        {
            List<string> permissions;

            if (role.Name is "Admin" or "SuperAdmin")
                permissions = EventNestPermissions.AllNames.ToList();
            else if (EventNestPermissions.RoleDefaults.TryGetValue(role.Name, out var defaults))
                permissions = defaults;
            else
                permissions = new List<string>();

            entries.AddRange(permissions.Distinct().Select(p => RolePermission.Create(role.Id, p)));
        }

        await context.RolePermissions.AddRangeAsync(entries);
        await context.SaveChangesAsync();
    }
}
