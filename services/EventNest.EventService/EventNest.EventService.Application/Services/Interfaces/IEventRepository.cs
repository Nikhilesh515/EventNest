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
    Task<bool> ExistsByTitleAsync(string title, Guid? excludeId = null);
    Task UpdateAsync(Event evt);
    Task DeleteAsync(Event evt);
    Task<int> SaveChangesAsync();
}
