using EventNest.RSVPService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventNest.RSVPService.Infrastructure.Data.Configurations;

public class RsvpConfiguration : IEntityTypeConfiguration<Rsvp>
{
    public void Configure(EntityTypeBuilder<Rsvp> builder)
    {
        builder.ToTable("Rsvps");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.EventId).IsRequired();
        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.UserName).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Status).IsRequired();
        builder.Property(r => r.GuestCount).IsRequired();
        builder.Property(r => r.Notes).HasMaxLength(1000);
        builder.Property(r => r.RespondedAt).IsRequired();
        builder.HasIndex(r => r.EventId);
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => new { r.EventId, r.UserId }).IsUnique();
    }
}
