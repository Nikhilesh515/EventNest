namespace EventNest.EventService.Application.Services.Interfaces;

public interface IRsvpGrpcClient
{
    Task<Dictionary<Guid, RsvpCounts>> GetCountsAsync(List<Guid> eventIds);
}

public readonly record struct RsvpCounts(int Going, int Maybe);
