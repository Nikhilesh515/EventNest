namespace EventNest.RSVPService.Application.DTOs.Rsvps;

public record CreateRsvpRequestDto(int GuestCount, string? Notes);
