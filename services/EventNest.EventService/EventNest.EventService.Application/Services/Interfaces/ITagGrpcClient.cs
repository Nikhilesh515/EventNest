namespace EventNest.EventService.Application.Services.Interfaces;

public interface ITagGrpcClient
{
    Task<List<(Guid Id, string Name, string Color)>?> GetTagsAsync(List<Guid> tagIds);
}
