namespace EventNest.EventService.Application.DTOs.Events;

public record EventTagDto(Guid TagId, string TagName);

public record EventDto(
    Guid Id,
    string Title,
    string? Description,
    string? Location,
    DateTime StartsAt,
    DateTime EndsAt,
    int Capacity,
    Guid OrganizerId,
    string OrganizerName,
    string Status,
    string Visibility,
    int Going,
    DateTime CreatedAt,
    List<EventTagDto> Tags);
