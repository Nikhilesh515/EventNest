using EventNest.AuthService.Domain.Entities;
using EventNest.AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventNest.AuthService.API.SeedData;

public static class RoleSeedData
{
    public static async Task SeedAsync(AuthDbContext context)
    {
        if (await context.Roles.AnyAsync())
            return;

        var roles = new[]
        {
            Role.Create("User", "User", "Default role for all users.", 0),
            Role.Create("Organizer", "Organizer", "Can create and manage events.", 1),
            Role.Create("Moderator", "Moderator", "Can moderate content and view users.", 2),
            Role.Create("Admin", "Admin", "Full access to all permissions.", 3),
            Role.Create("SuperAdmin", "Super Admin", "Unrestricted access, can manage roles.", 4)
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }
}
