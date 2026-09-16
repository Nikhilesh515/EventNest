using EventNest.EventService.Application.Services.Interfaces;
using EventNest.Shared.Infrastructure.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace EventNest.EventService.Infrastructure.Grpc;

public class TagGrpcClient : ITagGrpcClient
{
    private readonly TagService.TagServiceClient _client;
    private readonly ILogger<TagGrpcClient> _logger;

    public TagGrpcClient(TagService.TagServiceClient client, ILogger<TagGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<List<(Guid Id, string Name, string Color)>?> GetTagsAsync(List<Guid> tagIds)
    {
        try
        {
            var request = new GetTagsRequest();
            request.TagIds.AddRange(tagIds.Select(id => id.ToString()));

            var response = await _client.GetTagsAsync(request);

            return response.Tags
                .Where(t => Guid.TryParse(t.TagId, out _))
                .Select(t => (Guid.Parse(t.TagId), t.Name, t.Color))
                .ToList();
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to get tags via gRPC.");
            return null;
        }
    }
}
