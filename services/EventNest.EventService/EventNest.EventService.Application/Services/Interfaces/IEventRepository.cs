using EventNest.EventService.Application.DTOs.Events;
using EventNest.EventService.Domain.Entities;
using EventNest.EventService.Domain.Enums;

namespace EventNest.EventService.Application.Services.Interfaces;

public interface IEventRepository
{
    Task<Event> AddAsync(Event evt);
    Task<Event?> GetByIdAsync(Guid id);
    Task<List<Event>> GetAllAsync();
    Task<List<Event>> GetByOrganizerAsync(Guid organizerId);
    Task<List<Event>> GetByStatusAsync(EventStatus status);
    Task<(List<Event> Items, int Total)> QueryAsync(EventListQueryDto query, bool includeUnpublished);
    Task<List<Event>> QueryFilteredAsync(EventListQueryDto query, bool includeUnpublished);
    Task<bool> ExistsByTitleAsync(string title, Guid? excludeId = null);
    Task UpdateAsync(Event evt);
    Task DeleteAsync(Event evt);
    Task<int> SaveChangesAsync();
}
