namespace EventNest.EventService.Application.DTOs.Events;

public record UpdateEventRequestDto(
    string Title,
    string? Description,
    string? Location,
    DateTime StartsAt,
    DateTime EndsAt,
    int Capacity,
    List<EventTagRequestDto> Tags);
