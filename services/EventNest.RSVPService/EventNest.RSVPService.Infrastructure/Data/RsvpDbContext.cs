using EventNest.RSVPService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventNest.RSVPService.Infrastructure.Data;

public class RsvpDbContext : DbContext
{
    public DbSet<Rsvp> Rsvps => Set<Rsvp>();

    public RsvpDbContext(DbContextOptions<RsvpDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(RsvpDbContext).Assembly);
}
