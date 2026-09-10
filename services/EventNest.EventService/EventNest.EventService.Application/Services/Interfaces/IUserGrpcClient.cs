namespace EventNest.EventService.Application.Services.Interfaces;

public interface IUserGrpcClient
{
    Task<string?> GetDisplayNameAsync(Guid userId);
}
