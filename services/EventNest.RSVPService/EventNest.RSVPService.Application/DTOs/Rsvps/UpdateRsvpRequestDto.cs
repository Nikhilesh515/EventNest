namespace EventNest.RSVPService.Application.DTOs.Rsvps;

public record UpdateRsvpRequestDto(string? Status, int? GuestCount, string? Notes);
