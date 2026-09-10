namespace EventNest.EventService.Application.Services.Interfaces;

public interface ITagGrpcClient
{
    Task<List<(Guid Id, string Name)>?> GetTagsAsync(List<Guid> tagIds);
}
