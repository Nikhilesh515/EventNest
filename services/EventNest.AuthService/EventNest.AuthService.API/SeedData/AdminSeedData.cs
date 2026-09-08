using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Domain.Entities;
using EventNest.AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventNest.AuthService.API.SeedData;

public static class AdminSeedData
{
    public static async Task SeedAsync(AuthDbContext context, IPasswordHasher passwordHasher)
    {
        if (await context.Users.AnyAsync(u => u.Email == "admin@eventnest.io"))
            return;

        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole is null)
            return;

        var passwordHash = passwordHasher.Hash("Admin@123");
        var admin = User.Create("admin@eventnest.io", "Admin User", passwordHash, adminRole.Id);

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
    }
}
