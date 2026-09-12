using EventNest.EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventNest.EventService.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Location)
            .HasMaxLength(300);

        builder.Property(e => e.OrganizerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Visibility)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(e => e.OrganizerId);
        builder.HasIndex(e => e.StartsAt);
        builder.HasIndex(e => e.Status);

        builder.Ignore(e => e.EventTags);
    }
}
