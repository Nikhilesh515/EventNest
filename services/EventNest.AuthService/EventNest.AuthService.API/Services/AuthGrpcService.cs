using EventNest.Shared.Grpc;
using EventNest.AuthService.Application.Authorization;
using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Application.Services.Interfaces;
using Grpc.Core;

namespace EventNest.AuthService.API.Services;

public class AuthGrpcService : EventNest.Shared.Grpc.AuthService.AuthServiceBase
{
    private readonly IUserService _userService;
    private readonly IPermissionService _permissionService;
    private readonly IPermissionChecker _permissionChecker;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthGrpcService(
        IUserService userService,
        IPermissionService permissionService,
        IPermissionChecker permissionChecker,
        IJwtTokenService jwtTokenService)
    {
        _userService = userService;
        _permissionService = permissionService;
        _permissionChecker = permissionChecker;
        _jwtTokenService = jwtTokenService;
    }

    public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID."));

        var user = await _userService.GetByIdAsync(userId);
        if (user is null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found."));

        return new GetUserResponse
        {
            UserId = user.Id.ToString(),
            Email = user.Email,
            DisplayName = user.DisplayName,
            RoleName = user.RoleName
        };
    }

    public override async Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
    {
        var response = new GetUsersResponse();

        foreach (var userIdStr in request.UserIds)
        {
            if (Guid.TryParse(userIdStr, out var userId))
            {
                var user = await _userService.GetByIdAsync(userId);
                if (user is not null)
                {
                    response.Users.Add(new GetUserResponse
                    {
                        UserId = user.Id.ToString(),
                        Email = user.Email,
                        DisplayName = user.DisplayName,
                        RoleName = user.RoleName
                    });
                }
            }
        }

        return response;
    }

    public override async Task<CheckPermissionResponse> CheckPermission(CheckPermissionRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID."));

        var allowed = await _permissionChecker.CheckAsync(userId, request.Permission);

        return new CheckPermissionResponse
        {
            Allowed = allowed,
            Reason = allowed ? "Permission granted." : "Permission denied."
        };
    }

    public override async Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
    {
        var userId = _jwtTokenService.GetUserIdFromToken(request.Token);
        if (userId is null)
        {
            return new ValidateTokenResponse
            {
                Valid = false,
                Error = "Invalid or expired token."
            };
        }

        var permissions = await _permissionService.GetUserPermissionsAsync(userId.Value);

        return new ValidateTokenResponse
        {
            Valid = true,
            UserId = userId.Value.ToString(),
            Permissions = { permissions.Select(p => p.Name) }
        };
    }

    public override async Task<GetUserPermissionsResponse> GetUserPermissions(GetUserPermissionsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID."));

        var permissions = await _permissionService.GetUserPermissionsAsync(userId);

        return new GetUserPermissionsResponse
        {
            UserId = request.UserId,
            Permissions = { permissions.Select(p => p.Name) }
        };
    }
}
