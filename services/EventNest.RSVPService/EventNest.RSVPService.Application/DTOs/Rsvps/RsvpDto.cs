namespace EventNest.RSVPService.Application.DTOs.Rsvps;

public record RsvpDto(
    Guid Id,
    Guid EventId,
    Guid UserId,
    string UserName,
    string Status,
    int GuestCount,
    string? Notes,
    DateTime RespondedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
