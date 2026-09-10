using EventNest.EventService.Application.Services.Interfaces;
using EventNest.Shared.Infrastructure.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace EventNest.EventService.Infrastructure.Grpc;

public class AuthGrpcClient : IUserGrpcClient
{
    private readonly AuthService.AuthServiceClient _client;
    private readonly ILogger<AuthGrpcClient> _logger;

    public AuthGrpcClient(AuthService.AuthServiceClient client, ILogger<AuthGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<string?> GetDisplayNameAsync(Guid userId)
    {
        try
        {
            var response = await _client.GetUserAsync(new GetUserRequest
            {
                UserId = userId.ToString()
            });
            return response.DisplayName;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            _logger.LogWarning("User {UserId} not found via gRPC.", userId);
            return null;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to get user {UserId} via gRPC.", userId);
            return "Unknown User";
        }
    }
}
