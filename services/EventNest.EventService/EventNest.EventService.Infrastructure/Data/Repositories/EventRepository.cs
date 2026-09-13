using EventNest.EventService.Application.DTOs.Events;
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

    public async Task<(List<Event> Items, int Total)> QueryAsync(
        EventListQueryDto query, bool includeUnpublished)
    {
        var q = BuildQueryable(query, includeUnpublished);
        var total = await q.CountAsync();

        if (query.Sort?.ToLower() == "popularity")
        {
            var allItems = await q.OrderByDescending(e => e.CreatedAt).ToListAsync();
            return (allItems, total);
        }

        q = ApplySort(q, query.Sort);
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Max(query.PageSize, 1);
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<Event>> QueryFilteredAsync(
        EventListQueryDto query, bool includeUnpublished)
    {
        var q = BuildQueryable(query, includeUnpublished);
        q = ApplySort(q, query.Sort);
        return await q.ToListAsync();
    }

    private IQueryable<Event> BuildQueryable(EventListQueryDto query, bool includeUnpublished)
    {
        var q = _context.Events.Include(e => e.EventTags).AsQueryable();

        if (!includeUnpublished)
            q = q.Where(e => e.Status == EventStatus.Published);

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(e =>
                EF.Functions.ILike(e.Title, $"%{query.Search}%") ||
                EF.Functions.ILike(e.Description ?? "", $"%{query.Search}%") ||
                EF.Functions.ILike(e.Location ?? "", $"%{query.Search}%"));

        if (query.TagId is { Count: > 0 })
            q = q.Where(e => e.EventTags.Any(t => query.TagId.Contains(t.TagId)));

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<EventStatus>(query.Status, true, out var status))
            q = q.Where(e => e.Status == status);

        if (!string.IsNullOrWhiteSpace(query.Visibility) &&
            Enum.TryParse<EventVisibility>(query.Visibility, true, out var visibility))
            q = q.Where(e => e.Visibility == visibility);

        if (query.Timeframe?.ToLower() == "upcoming")
            q = q.Where(e => e.EndsAt >= DateTime.UtcNow);
        else if (query.Timeframe?.ToLower() == "past")
            q = q.Where(e => e.EndsAt < DateTime.UtcNow);

        return q;
    }

    private static IQueryable<Event> ApplySort(IQueryable<Event> q, string? sort)
    {
        return sort?.ToLower() switch
        {
            "date-asc" => q.OrderBy(e => e.StartsAt),
            "date-desc" => q.OrderByDescending(e => e.StartsAt),
            "created-desc" => q.OrderByDescending(e => e.CreatedAt),
            "popularity" => q.OrderByDescending(e => e.CreatedAt),
            _ => q.OrderByDescending(e => e.CreatedAt)
        };
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
