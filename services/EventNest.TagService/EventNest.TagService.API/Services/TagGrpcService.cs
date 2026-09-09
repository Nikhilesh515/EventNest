using EventNest.TagService.Application.DTOs.Tags;
using EventNest.TagService.Application.Services.Interfaces;
using EventNest.Shared.Infrastructure.Grpc;
using Grpc.Core;

namespace EventNest.TagService.API.Services;

public class TagGrpcService : EventNest.Shared.Infrastructure.Grpc.TagService.TagServiceBase
{
    private readonly ITagService _tagService;

    public TagGrpcService(ITagService tagService)
    {
        _tagService = tagService;
    }

    public override async Task<GetTagResponse> GetTag(GetTagRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.TagId, out var tagId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid tag ID."));

        var tag = await _tagService.GetByIdAsync(tagId);
        if (tag is null)
            throw new RpcException(new Status(StatusCode.NotFound, "Tag not found."));

        return new GetTagResponse
        {
            TagId = tag.Id.ToString(),
            Name = tag.Name,
            Color = tag.Color
        };
    }

    public override async Task<GetTagsResponse> GetTags(GetTagsRequest request, ServerCallContext context)
    {
        var response = new GetTagsResponse();

        foreach (var tagIdStr in request.TagIds)
        {
            if (Guid.TryParse(tagIdStr, out var tagId))
            {
                var tag = await _tagService.GetByIdAsync(tagId);
                if (tag is not null)
                {
                    response.Tags.Add(new GetTagResponse
                    {
                        TagId = tag.Id.ToString(),
                        Name = tag.Name,
                        Color = tag.Color
                    });
                }
            }
        }

        return response;
    }
}
