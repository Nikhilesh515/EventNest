using EventNest.AuthService.Application.DTOs.Auth;
using EventNest.AuthService.Application.DTOs.Users;
using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Application.Services.Interfaces;
using EventNest.AuthService.Domain.Entities;
using EventNest.AuthService.Domain.Exceptions;

namespace EventNest.AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPermissionStore _permissionStore;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        IPermissionStore permissionStore)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _refreshTokenRepository = refreshTokenRepository;
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
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var refreshToken = RefreshToken.Create(
            refreshTokenValue,
            DateTime.UtcNow.AddDays(30),
            "system",
            user.Id);
        await _refreshTokenRepository.AddAsync(refreshToken);

        return new AuthResponseDto(
            accessToken,
            refreshTokenValue,
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
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var refreshToken = RefreshToken.Create(
            refreshTokenValue,
            DateTime.UtcNow.AddDays(30),
            "system",
            user.Id);
        await _refreshTokenRepository.AddAsync(refreshToken);

        return new AuthResponseDto(
            accessToken,
            refreshTokenValue,
            3600,
            new UserDto(user.Id, user.Email, user.DisplayName, roleName, user.IsActive));
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (storedToken is null || !storedToken.IsActive)
            throw new UnauthorizedException("Invalid refresh token.");

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user is null)
            throw new NotFoundException("User not found.");

        if (!user.IsActive)
            throw new UnauthorizedException("User account is deactivated.");

        // Revoke old refresh token
        await _refreshTokenRepository.RevokeAsync(refreshToken, "system");

        var role = await _roleRepository.GetByIdAsync(user.RoleId);
        var roleName = role?.Name ?? "User";

        var permissions = await _permissionStore.GetUserPermissionsAsync(user.Id);
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user, permissions);
        var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var newRefreshToken = RefreshToken.Create(
            newRefreshTokenValue,
            DateTime.UtcNow.AddDays(30),
            "system",
            user.Id);
        await _refreshTokenRepository.AddAsync(newRefreshToken);

        return new AuthResponseDto(
            newAccessToken,
            newRefreshTokenValue,
            3600,
            new UserDto(user.Id, user.Email, user.DisplayName, roleName, user.IsActive));
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _refreshTokenRepository.RevokeAsync(refreshToken, "system");
    }
}
