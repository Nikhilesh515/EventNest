using EventNest.RSVPService.Application.Services.Interfaces;
using EventNest.Shared.Infrastructure.Grpc;
using Grpc.Core;

namespace EventNest.RSVPService.API.Services;

public class RsvpGrpcService : RsvpService.RsvpServiceBase
{
    private readonly IRsvpService _rsvpService;

    public RsvpGrpcService(IRsvpService rsvpService) => _rsvpService = rsvpService;

    public override async Task<GetRsvpCountResponse> GetRsvpCount(GetRsvpCountRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.EventId, out var eventId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid event ID."));

        var confirmed = await _rsvpService.GetConfirmedCountAsync(eventId);
        var total = await _rsvpService.GetTotalGuestsAsync(eventId);
        var counts = await _rsvpService.GetStatusCountsAsync(new List<Guid> { eventId });
        counts.TryGetValue(eventId, out var statusCounts);

        return new GetRsvpCountResponse
        {
            EventId = request.EventId,
            ConfirmedCount = confirmed,
            TotalGuests = total,
            MaybeCount = statusCounts.Maybe
        };
    }

    public override async Task<GetRsvpCountsResponse> GetRsvpCounts(GetRsvpCountsRequest request, ServerCallContext context)
    {
        var eventIds = request.EventIds
            .Select(id => Guid.TryParse(id, out var g) ? (Guid?)g : null)
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToList();

        var counts = await _rsvpService.GetStatusCountsAsync(eventIds);

        var response = new GetRsvpCountsResponse();
        foreach (var id in eventIds)
        {
            counts.TryGetValue(id, out var statusCounts);
            response.Counts.Add(new RsvpCountItem
            {
                EventId = id.ToString(),
                ConfirmedCount = statusCounts.Confirmed,
                MaybeCount = statusCounts.Maybe
            });
        }

        return response;
    }

    public override async Task<GetUserRsvpStatusResponse> GetUserRsvpStatus(GetUserRsvpStatusRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId) || !Guid.TryParse(request.EventId, out var eventId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user or event ID."));

        var rsvps = await _rsvpService.GetByUserIdAsync(userId);
        var rsvp = rsvps.FirstOrDefault(r => r.EventId == eventId);

        return new GetUserRsvpStatusResponse
        {
            HasRsvp = rsvp is not null,
            RsvpId = rsvp?.Id.ToString() ?? "",
            Status = rsvp?.Status ?? "",
            GuestCount = rsvp?.GuestCount ?? 0
        };
    }

    public override async Task<GetEventRsvpsResponse> GetEventRsvps(GetEventRsvpsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.EventId, out var eventId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid event ID."));

        var rsvps = await _rsvpService.GetByEventIdAsync(eventId);
        var response = new GetEventRsvpsResponse { Count = rsvps.Count };
        response.Rsvps.AddRange(rsvps.Select(r => new RsvpItem
        {
            RsvpId = r.Id.ToString(),
            EventId = r.EventId.ToString(),
            UserId = r.UserId.ToString(),
            UserName = r.UserName,
            Status = r.Status,
            GuestCount = r.GuestCount
        }));
        return response;
    }

    public override async Task<GetUserRsvpsResponse> GetUserRsvps(GetUserRsvpsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID."));

        var rsvps = await _rsvpService.GetByUserIdAsync(userId);
        var response = new GetUserRsvpsResponse { Count = rsvps.Count };
        response.Rsvps.AddRange(rsvps.Select(r => new RsvpItem
        {
            RsvpId = r.Id.ToString(),
            EventId = r.EventId.ToString(),
            UserId = r.UserId.ToString(),
            UserName = r.UserName,
            Status = r.Status,
            GuestCount = r.GuestCount
        }));
        return response;
    }
}
