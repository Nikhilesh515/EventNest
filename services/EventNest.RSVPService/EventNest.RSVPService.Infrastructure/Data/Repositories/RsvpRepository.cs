using EventNest.RSVPService.Application.Services.Interfaces;
using EventNest.RSVPService.Domain.Entities;
using EventNest.RSVPService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventNest.RSVPService.Infrastructure.Data.Repositories;

public class RsvpRepository : IRsvpRepository
{
    private readonly RsvpDbContext _context;

    public RsvpRepository(RsvpDbContext context) => _context = context;

    public async Task<Rsvp> AddAsync(Rsvp rsvp)
    {
        await _context.Rsvps.AddAsync(rsvp);
        return rsvp;
    }

    public async Task<Rsvp?> GetByIdAsync(Guid id)
        => await _context.Rsvps.FindAsync(id);

    public async Task<List<Rsvp>> GetByEventIdAsync(Guid eventId)
        => await _context.Rsvps.Where(r => r.EventId == eventId).ToListAsync();

    public async Task<List<Rsvp>> GetByUserIdAsync(Guid userId)
        => await _context.Rsvps.Where(r => r.UserId == userId).ToListAsync();

    public async Task<Rsvp?> GetByEventAndUserAsync(Guid eventId, Guid userId)
        => await _context.Rsvps.FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

    public async Task<int> GetNonCancelledGuestCountAsync(Guid eventId)
        => await _context.Rsvps
            .Where(r => r.EventId == eventId && r.Status != RsvpStatus.Cancelled)
            .SumAsync(r => r.GuestCount);

    public async Task<int> GetConfirmedCountAsync(Guid eventId)
        => await _context.Rsvps
            .Where(r => r.EventId == eventId && r.Status == RsvpStatus.Confirmed)
            .SumAsync(r => r.GuestCount);

    public async Task<Dictionary<Guid, int>> GetConfirmedCountsAsync(List<Guid> eventIds)
        => await _context.Rsvps
            .Where(r => eventIds.Contains(r.EventId) && r.Status == RsvpStatus.Confirmed)
            .GroupBy(r => r.EventId)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(r => r.GuestCount));

    public async Task<Dictionary<Guid, (int Confirmed, int Maybe)>> GetStatusCountsAsync(List<Guid> eventIds)
    {
        var rows = await _context.Rsvps
            .Where(r => eventIds.Contains(r.EventId) &&
                        (r.Status == RsvpStatus.Confirmed || r.Status == RsvpStatus.Maybe))
            .GroupBy(r => r.EventId)
            .Select(g => new
            {
                EventId = g.Key,
                Confirmed = g.Where(r => r.Status == RsvpStatus.Confirmed).Sum(r => r.GuestCount),
                Maybe = g.Where(r => r.Status == RsvpStatus.Maybe).Sum(r => r.GuestCount)
            })
            .ToListAsync();

        return rows.ToDictionary(r => r.EventId, r => (r.Confirmed, r.Maybe));
    }

    public Task UpdateAsync(Rsvp rsvp)
    {
        _context.Rsvps.Update(rsvp);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Rsvp rsvp)
    {
        _context.Rsvps.Remove(rsvp);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
