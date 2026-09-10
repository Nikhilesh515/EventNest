using EventNest.EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventNest.EventService.Infrastructure.Data;

public class EventDbContext : DbContext
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventTag> EventTags => Set<EventTag>();

    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventDbContext).Assembly);
    }
}
