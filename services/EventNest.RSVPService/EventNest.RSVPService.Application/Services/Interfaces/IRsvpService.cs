using EventNest.RSVPService.Application.DTOs.Rsvps;

namespace EventNest.RSVPService.Application.Services.Interfaces;

public interface IRsvpService
{
    Task<RsvpDto> CreateAsync(Guid eventId, Guid userId, string userName, CreateRsvpRequestDto request);
    Task<RsvpDto?> GetByIdAsync(Guid id);
    Task<List<RsvpDetailDto>> GetByEventIdAsync(Guid eventId);
    Task<List<RsvpDetailDto>> GetByUserIdAsync(Guid userId);
    Task<RsvpDto> UpdateAsync(Guid id, Guid userId, UpdateRsvpRequestDto request);
    Task CancelAsync(Guid id, Guid userId);
    Task<int> GetConfirmedCountAsync(Guid eventId);
    Task<Dictionary<Guid, int>> GetConfirmedCountsAsync(List<Guid> eventIds);
    Task<Dictionary<Guid, (int Confirmed, int Maybe)>> GetStatusCountsAsync(List<Guid> eventIds);
    Task<int> GetTotalGuestsAsync(Guid eventId);
}
