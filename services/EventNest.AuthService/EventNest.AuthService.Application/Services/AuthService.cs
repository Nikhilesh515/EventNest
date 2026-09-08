using EventNest.AuthService.Application.DTOs;
using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Application.Services.Interfaces;
using EventNest.AuthService.Domain.Entities;
using EventNest.AuthService.Domain.Exceptions;

namespace EventNest.AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPermissionStore _permissionStore;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        IPermissionStore permissionStore)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _permissionStore = permissionStore;
    }

    public async Task<AuthResponseDto> RegisterAsync(string email, string displayName, string password)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser is not null)
            throw new ConflictException($"User with email '{email}' is already registered.");

        var defaultRole = await _roleRepository.GetByNameAsync("User");
        if (defaultRole is null)
            throw new NotFoundException("Default role 'User' not found.");

        var passwordHash = _passwordHasher.Hash(password);
        var user = User.Create(email, displayName, passwordHash, defaultRole.Id);

        await _userRepository.AddAsync(user);

        var permissions = await _permissionStore.GetUserPermissionsAsync(user.Id);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, permissions);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        return new AuthResponseDto(
            accessToken,
            refreshToken,
            3600,
            new UserDto(user.Id, user.Email, user.DisplayName, defaultRole.Name, user.IsActive));
    }

    public async Task<AuthResponseDto> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
            throw new UnauthorizedException("Invalid email or password.");

        if (!_passwordHasher.Verify(password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedException("User account is deactivated.");

        var role = await _roleRepository.GetByIdAsync(user.RoleId);
        var roleName = role?.Name ?? "User";

        var permissions = await _permissionStore.GetUserPermissionsAsync(user.Id);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, permissions);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        return new AuthResponseDto(
            accessToken,
            refreshToken,
            3600,
            new UserDto(user.Id, user.Email, user.DisplayName, roleName, user.IsActive));
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var userId = _jwtTokenService.GetUserIdFromToken(refreshToken);
        if (userId is null)
            throw new UnauthorizedException("Invalid refresh token.");

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user is null)
            throw new NotFoundException("User not found.");

        if (!user.IsActive)
            throw new UnauthorizedException("User account is deactivated.");

        var role = await _roleRepository.GetByIdAsync(user.RoleId);
        var roleName = role?.Name ?? "User";

        var permissions = await _permissionStore.GetUserPermissionsAsync(user.Id);
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user, permissions);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        return new AuthResponseDto(
            newAccessToken,
            newRefreshToken,
            3600,
            new UserDto(user.Id, user.Email, user.DisplayName, roleName, user.IsActive));
    }

    public Task LogoutAsync(string refreshToken)
    {
        // In a real implementation, we would revoke the refresh token in the database
        // For now, this is a no-op as refresh tokens are not stored in our current schema
        return Task.CompletedTask;
    }
}
