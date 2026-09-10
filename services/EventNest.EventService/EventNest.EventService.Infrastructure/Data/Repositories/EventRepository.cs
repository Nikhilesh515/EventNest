using EventNest.EventService.Application.Services.Interfaces;
using EventNest.EventService.Domain.Entities;
using EventNest.EventService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventNest.EventService.Infrastructure.Data.Repositories;

public class EventRepository : IEventRepository
{
    private readonly EventDbContext _context;

    public EventRepository(EventDbContext context)
    {
        _context = context;
    }

    public async Task<Event> AddAsync(Event evt)
    {
        await _context.Events.AddAsync(evt);
        return evt;
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        return await _context.Events
            .Include(e => e.EventTags)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<Event>> GetAllAsync()
    {
        return await _context.Events
            .Include(e => e.EventTags)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Event>> GetByOrganizerAsync(Guid organizerId)
    {
        return await _context.Events
            .Include(e => e.EventTags)
            .Where(e => e.OrganizerId == organizerId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Event>> GetByStatusAsync(EventStatus status)
    {
        return await _context.Events
            .Include(e => e.EventTags)
            .Where(e => e.Status == status)
            .OrderByDescending(e => e.StartsAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsByTitleAsync(string title, Guid? excludeId = null)
    {
        return await _context.Events.AnyAsync(e =>
            e.Title == title && (excludeId == null || e.Id != excludeId));
    }

    public async Task UpdateAsync(Event evt)
    {
        _context.Events.Update(evt);
    }

    public async Task DeleteAsync(Event evt)
    {
        _context.Events.Remove(evt);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
