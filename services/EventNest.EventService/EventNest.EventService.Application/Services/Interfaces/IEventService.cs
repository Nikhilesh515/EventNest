using EventNest.EventService.Application.DTOs.Events;

namespace EventNest.EventService.Application.Services.Interfaces;

public interface IEventService
{
    Task<EventDto> CreateAsync(CreateEventRequestDto request, Guid organizerId, string organizerName);
    Task<EventDto?> GetByIdAsync(Guid id);
    Task<List<EventDto>> GetAllAsync();
    Task<List<EventDto>> GetByOrganizerAsync(Guid organizerId);
    Task<List<EventDto>> GetByStatusAsync(string status);
    Task<EventDto> UpdateAsync(Guid id, UpdateEventRequestDto request);
    Task DeleteAsync(Guid id);
    Task<EventDto> PublishAsync(Guid id);
    Task<EventDto> CancelAsync(Guid id);
    Task<EventDto> CompleteAsync(Guid id);
}
