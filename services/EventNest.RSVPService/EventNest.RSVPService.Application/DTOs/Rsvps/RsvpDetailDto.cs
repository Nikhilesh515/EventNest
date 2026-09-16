namespace EventNest.RSVPService.Application.DTOs.Rsvps;

public record RsvpDetailDto(
    Guid Id,
    Guid EventId,
    Guid UserId,
    string UserName,
    string Status,
    int GuestCount,
    string? Notes,
    DateTime RespondedAt,
    DateTime CreatedAt,
    string? EventTitle,
    DateTime? EventStartsAt,
    string? EventLocation
);
