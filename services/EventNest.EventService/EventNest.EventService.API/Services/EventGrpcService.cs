using EventNest.EventService.Application.Services.Interfaces;
using EventNest.Shared.Infrastructure.Grpc;
using Grpc.Core;

namespace EventNest.EventService.API.Services;

public class EventGrpcService : EventNest.Shared.Infrastructure.Grpc.EventService.EventServiceBase
{
    private readonly IEventService _eventService;

    public EventGrpcService(IEventService eventService)
    {
        _eventService = eventService;
    }

    public override async Task<GetEventResponse> GetEvent(GetEventRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.EventId, out var eventId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid event ID."));

        var evt = await _eventService.GetByIdAsync(eventId);
        if (evt is null)
            throw new RpcException(new Status(StatusCode.NotFound, "Event not found."));

        return new GetEventResponse
        {
            EventId = evt.Id.ToString(),
            Title = evt.Title,
            OrganizerId = evt.OrganizerId.ToString(),
            MaxAttendees = evt.Capacity,
            Status = evt.Status,
            StartsAt = evt.StartsAt.ToString("o"),
            Location = evt.Location ?? string.Empty
        };
    }

    public override async Task<GetEventsByCreatorResponse> GetEventsByCreator(
        GetEventsByCreatorRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.CreatorId, out var creatorId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid creator ID."));

        var events = await _eventService.GetByOrganizerAsync(creatorId);

        var response = new GetEventsByCreatorResponse();
        response.Events.AddRange(events.Select(e => new GetEventResponse
        {
            EventId = e.Id.ToString(),
            Title = e.Title,
            OrganizerId = e.OrganizerId.ToString(),
            MaxAttendees = e.Capacity,
            Status = e.Status,
            StartsAt = e.StartsAt.ToString("o"),
            Location = e.Location ?? string.Empty
        }));

        return response;
    }
}
