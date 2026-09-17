using EventNest.AuthService.Application.DTOs.Auth;
using EventNest.AuthService.Application.DTOs.Users;
using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Application.Options;
using EventNest.AuthService.Application.Services.Interfaces;
using EventNest.AuthService.Domain.Entities;
using EventNest.Shared.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace EventNest.AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPermissionStore _permissionStore;
    private readonly ITokenHasher _tokenHasher;
    private readonly AuthOptions _options;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        IPermissionStore permissionStore,
        ITokenHasher tokenHasher,
        IOptions<AuthOptions> options)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _permissionStore = permissionStore;
        _tokenHasher = tokenHasher;
        _options = options.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(string email, string displayName, string password, string? ipAddress)
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

        await _permissionStore.GetUserPermissionsAsync(user.Id);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, defaultRole.Name);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var refreshToken = RefreshToken.Create(
            _tokenHasher.Hash(refreshTokenValue),
            _jwtTokenService.GetRefreshTokenExpiryUtc(),
            ipAddress,
            user.Id);
        await _refreshTokenRepository.AddAsync(refreshToken);

        return new AuthResponseDto(
            accessToken,
            _jwtTokenService.GetAccessTokenExpirySeconds(),
            new UserDto(user.Id, user.Email, user.DisplayName, defaultRole.Name, user.RoleId, user.IsActive))
        {
            RefreshTokenValue = refreshTokenValue
        };
    }

    public async Task<AuthResponseDto> LoginAsync(string email, string password, string? ipAddress)
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

        await _permissionStore.GetUserPermissionsAsync(user.Id);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roleName);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var refreshToken = RefreshToken.Create(
            _tokenHasher.Hash(refreshTokenValue),
            _jwtTokenService.GetRefreshTokenExpiryUtc(),
            ipAddress,
            user.Id);
        await _refreshTokenRepository.AddAsync(refreshToken);

        return new AuthResponseDto(
            accessToken,
            _jwtTokenService.GetAccessTokenExpirySeconds(),
            new UserDto(user.Id, user.Email, user.DisplayName, roleName, user.RoleId, user.IsActive))
        {
            RefreshTokenValue = refreshTokenValue
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress)
    {
        var tokenHash = _tokenHasher.Hash(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (storedToken is null)
            throw new UnauthorizedException("Invalid refresh token.");

        if (storedToken.IsExpired)
            throw new UnauthorizedException("Refresh token has expired.");

        if (storedToken.IsRevoked)
        {
            var rotatedRecently = storedToken.ReplacedByTokenHash is not null
                && storedToken.RevokedAt.HasValue
                && (DateTime.UtcNow - storedToken.RevokedAt.Value).TotalSeconds < _options.RefreshRotationGraceSeconds;

            if (rotatedRecently)
            {
                // Multi-tab grace: allow through, re-rotate
            }
            else if (storedToken.ReplacedByTokenHash is not null)
            {
                // Reuse detected: revoke ALL active tokens for this user
                await _refreshTokenRepository.RevokeAllActiveByUserIdAsync(storedToken.UserId);
                throw new UnauthorizedException("Refresh token reuse detected. All sessions revoked.");
            }
            else
            {
                throw new UnauthorizedException("Refresh token has been revoked.");
            }
        }

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user is null)
            throw new NotFoundException("User not found.");

        if (!user.IsActive)
            throw new UnauthorizedException("User account is deactivated.");

        var role = await _roleRepository.GetByIdAsync(user.RoleId);
        var roleName = role?.Name ?? "User";

        var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _tokenHasher.Hash(newRefreshTokenValue);

        if (!storedToken.IsRevoked)
        {
            await _refreshTokenRepository.RevokeAsync(tokenHash, newRefreshTokenHash);
        }

        var newRefreshToken = RefreshToken.Create(
            newRefreshTokenHash,
            _jwtTokenService.GetRefreshTokenExpiryUtc(),
            ipAddress,
            user.Id);
        await _refreshTokenRepository.AddAsync(newRefreshToken);

        await _permissionStore.GetUserPermissionsAsync(user.Id);
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roleName);

        return new AuthResponseDto(
            newAccessToken,
            _jwtTokenService.GetAccessTokenExpirySeconds(),
            new UserDto(user.Id, user.Email, user.DisplayName, roleName, user.RoleId, user.IsActive))
        {
            RefreshTokenValue = newRefreshTokenValue
        };
    }

    public async Task LogoutAsync(string refreshToken, string? ipAddress)
    {
        var tokenHash = _tokenHasher.Hash(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);
        if (storedToken is not null && !storedToken.IsRevoked)
        {
            await _refreshTokenRepository.RevokeAsync(tokenHash);
        }
    }
}
