namespace EventNest.RSVPService.Application.Services.Interfaces;

public interface IEventGrpcClient
{
    Task<EventGrpcInfo?> GetEventAsync(Guid eventId);
}

public record EventGrpcInfo(Guid Id, string Title, Guid OrganizerId, int MaxAttendees, string Status);
