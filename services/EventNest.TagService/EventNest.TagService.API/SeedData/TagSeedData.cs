using EventNest.TagService.Domain.Entities;
using EventNest.TagService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventNest.TagService.API.SeedData;

public static class TagSeedData
{
    public static async Task SeedAsync(TagDbContext context)
    {
        if (await context.Tags.AnyAsync())
            return;

        var tags = new[]
        {
            Tag.Create("Technology", "#3b82f6"),
            Tag.Create("Music", "#ef4444"),
            Tag.Create("Food & Drink", "#f59e0b"),
            Tag.Create("Sports", "#10b981"),
            Tag.Create("Networking", "#8b5cf6")
        };

        await context.Tags.AddRangeAsync(tags);
        await context.SaveChangesAsync();
    }
}
