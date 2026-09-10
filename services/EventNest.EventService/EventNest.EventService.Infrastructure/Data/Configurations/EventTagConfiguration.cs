using EventNest.EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventNest.EventService.Infrastructure.Data.Configurations;

public class EventTagConfiguration : IEntityTypeConfiguration<EventTag>
{
    public void Configure(EntityTypeBuilder<EventTag> builder)
    {
        builder.ToTable("EventTags");

        builder.HasKey(e => new { e.EventId, e.TagId });

        builder.Property(e => e.TagName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne<Event>()
            .WithMany(e => e.EventTags)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
