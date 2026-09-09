using EventNest.TagService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventNest.TagService.Infrastructure.Data;

public class TagDbContext : DbContext
{
    public DbSet<Tag> Tags => Set<Tag>();

    public TagDbContext(DbContextOptions<TagDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TagDbContext).Assembly);
    }
}
