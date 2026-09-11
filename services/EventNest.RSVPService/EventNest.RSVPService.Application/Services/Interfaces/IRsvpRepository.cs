using EventNest.RSVPService.Domain.Entities;

namespace EventNest.RSVPService.Application.Services.Interfaces;

public interface IRsvpRepository
{
    Task<Rsvp> AddAsync(Rsvp rsvp);
    Task<Rsvp?> GetByIdAsync(Guid id);
    Task<List<Rsvp>> GetByEventIdAsync(Guid eventId);
    Task<List<Rsvp>> GetByUserIdAsync(Guid userId);
    Task<Rsvp?> GetByEventAndUserAsync(Guid eventId, Guid userId);
    Task<int> GetNonCancelledGuestCountAsync(Guid eventId);
    Task UpdateAsync(Rsvp rsvp);
    Task DeleteAsync(Rsvp rsvp);
    Task<int> SaveChangesAsync();
}
