using EventNest.RSVPService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventNest.RSVPService.API.SeedData;

public static class RsvpSeedData
{
    public static async Task SeedAsync(RsvpDbContext context)
    {
        if (await context.Rsvps.AnyAsync())
            return;
    }
}
