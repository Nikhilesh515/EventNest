namespace EventNest.EventService.Application.DTOs.Events;

public record EventTagRequestDto(Guid TagId, string TagName);

public record CreateEventRequestDto(
    string Title,
    string? Description,
    string? Location,
    DateTime StartsAt,
    DateTime EndsAt,
    int Capacity,
    List<EventTagRequestDto> Tags,
    string? Visibility = null);
