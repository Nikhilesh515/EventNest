using EventNest.EventService.Application.Services.Interfaces;
using EventNest.Shared.Infrastructure.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace EventNest.EventService.Infrastructure.Grpc;

public class RsvpGrpcClient : IRsvpGrpcClient
{
    private readonly RsvpService.RsvpServiceClient _client;
    private readonly ILogger<RsvpGrpcClient> _logger;

    public RsvpGrpcClient(RsvpService.RsvpServiceClient client, ILogger<RsvpGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Dictionary<Guid, int>> GetGoingCountsAsync(List<Guid> eventIds)
    {
        try
        {
            var request = new GetRsvpCountsRequest();
            request.EventIds.AddRange(eventIds.Select(id => id.ToString()));

            var response = await _client.GetRsvpCountsAsync(request);

            return response.Counts
                .Where(c => Guid.TryParse(c.EventId, out _))
                .ToDictionary(c => Guid.Parse(c.EventId), c => c.ConfirmedCount);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to get RSVP counts via gRPC.");
            return new Dictionary<Guid, int>();
        }
    }
}
