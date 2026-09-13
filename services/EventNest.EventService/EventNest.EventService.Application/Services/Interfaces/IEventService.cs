using EventNest.EventService.Application.DTOs.Events;

namespace EventNest.EventService.Application.Services.Interfaces;

public interface IEventService
{
    Task<EventDto> CreateAsync(CreateEventRequestDto request, Guid organizerId, string organizerName);
    Task<EventDto?> GetByIdAsync(Guid id);
    Task<List<EventDto>> GetAllAsync();
    Task<PagedResultDto<EventDto>> GetPagedAsync(EventListQueryDto query, bool includeUnpublished);
    Task<List<EventDto>> GetByOrganizerAsync(Guid organizerId);
    Task<List<EventDto>> GetByStatusAsync(string status);
    Task<EventDto> UpdateAsync(Guid id, UpdateEventRequestDto request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<EventDto> PublishAsync(Guid id, Guid userId);
    Task<EventDto> CancelAsync(Guid id, Guid userId);
    Task<EventDto> CompleteAsync(Guid id, Guid userId);
}
