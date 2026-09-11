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
