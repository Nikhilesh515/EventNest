using EventNest.RSVPService.Application.Services.Interfaces;
using EventNest.Shared.Infrastructure.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace EventNest.RSVPService.Infrastructure.Grpc;

public class EventGrpcClient : IEventGrpcClient
{
    private readonly EventService.EventServiceClient _client;
    private readonly ILogger<EventGrpcClient> _logger;

    public EventGrpcClient(EventService.EventServiceClient client, ILogger<EventGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<EventGrpcInfo?> GetEventAsync(Guid eventId)
    {
        try
        {
            var response = await _client.GetEventAsync(new GetEventRequest { EventId = eventId.ToString() });

            DateTime? startsAt = null;
            if (DateTime.TryParse(
                    response.StartsAt,
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var parsed))
            {
                startsAt = parsed;
            }

            return new EventGrpcInfo(
                Guid.Parse(response.EventId),
                response.Title,
                Guid.Parse(response.OrganizerId),
                response.MaxAttendees,
                response.Status,
                startsAt,
                string.IsNullOrEmpty(response.Location) ? null : response.Location);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            _logger.LogWarning("Event {EventId} not found via gRPC.", eventId);
            return null;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to get event {EventId} via gRPC.", eventId);
            return null;
        }
    }
}
