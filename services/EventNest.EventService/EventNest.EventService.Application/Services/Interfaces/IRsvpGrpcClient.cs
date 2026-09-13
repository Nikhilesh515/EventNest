namespace EventNest.EventService.Application.Services.Interfaces;

public interface IRsvpGrpcClient
{
    Task<Dictionary<Guid, int>> GetGoingCountsAsync(List<Guid> eventIds);
}
