using EventNest.EventService.Domain.Entities;
using EventNest.EventService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventNest.EventService.API.SeedData;

public static class EventSeedData
{
    public static async Task SeedAsync(EventDbContext context)
    {
        if (await context.Events.AnyAsync())
            return;

        var organizerId = Guid.NewGuid();

        var techMeetup = Event.Create(
            "Tech Meetup 2026",
            "Monthly tech meetup for developers",
            "Convention Center",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(30).AddHours(3),
            100,
            organizerId,
            "System Admin");

        var foodFestival = Event.Create(
            "Food Festival",
            "Annual food festival featuring local restaurants",
            "City Park",
            DateTime.UtcNow.AddDays(60),
            DateTime.UtcNow.AddDays(60).AddHours(8),
            500,
            organizerId,
            "System Admin");
        foodFestival.Publish();

        var musicConcert = Event.Create(
            "Music Concert",
            "Live music event with local bands",
            "Arena",
            DateTime.UtcNow.AddDays(45),
            DateTime.UtcNow.AddDays(45).AddHours(4),
            200,
            organizerId,
            "System Admin");

        await context.Events.AddRangeAsync(techMeetup, foodFestival, musicConcert);
        await context.SaveChangesAsync();
    }
}
