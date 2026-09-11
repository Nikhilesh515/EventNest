using EventNest.RSVPService.Domain.Enums;
using EventNest.Shared.Domain.Entities;

namespace EventNest.RSVPService.Domain.Entities;

public class Rsvp : BaseEntity
{
    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public RsvpStatus Status { get; private set; }
    public int GuestCount { get; private set; }
    public string? Notes { get; private set; }
    public DateTime RespondedAt { get; private set; }

    private Rsvp() { }

    public static Rsvp Create(Guid eventId, Guid userId, string userName, int guestCount, string? notes)
    {
        return new Rsvp
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            UserName = userName,
            Status = RsvpStatus.Confirmed,
            GuestCount = guestCount > 0 ? guestCount : 1,
            Notes = notes,
            RespondedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Reactivate(string userName, int guestCount, string? notes)
    {
        Status = RsvpStatus.Confirmed;
        UserName = userName;
        GuestCount = guestCount > 0 ? guestCount : 1;
        Notes = notes;
        RespondedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        Status = RsvpStatus.Confirmed;
        RespondedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Decline()
    {
        Status = RsvpStatus.Declined;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Maybe()
    {
        Status = RsvpStatus.Maybe;
        RespondedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = RsvpStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateGuestCount(int count)
    {
        GuestCount = count > 0 ? count : 1;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}
