using EventNest.AuthService.Application.DTOs.Auth;
using EventNest.AuthService.Application.Options;
using EventNest.Shared.Application.DTOs;
using EventNest.AuthService.Application.Services.Interfaces;
using EventNest.Shared.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EventNest.AuthService.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AuthOptions _options;

    public AuthController(IAuthService authService, IOptions<AuthOptions> options)
    {
        _authService = authService;
        _options = options.Value;
    }

    private string? ClientIpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();

    private bool IsOriginAllowed()
    {
        var origin = Request.Headers.Origin.ToString();
        if (string.IsNullOrEmpty(origin))
            return true;

        return _options.AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(
            request.Email, request.DisplayName, request.Password, ClientIpAddress);
        SetRefreshTokenCookie(result.RefreshTokenValue);
        return Ok(ApiResponseDto<AuthResponseDto>.Ok(result));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password, ClientIpAddress);
        SetRefreshTokenCookie(result.RefreshTokenValue);
        return Ok(ApiResponseDto<AuthResponseDto>.Ok(result));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto? request = null)
    {
        if (!IsOriginAllowed())
            return StatusCode(403, ApiResponseDto<object>.Fail(403, "Request origin is not allowed."));

        var refreshToken = request?.RefreshToken;
        if (string.IsNullOrWhiteSpace(refreshToken))
            HttpContext.Request.Cookies.TryGetValue("eventnest.refresh_token", out refreshToken);

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            DeleteRefreshTokenCookie();
            return Unauthorized(ApiResponseDto<object>.Fail(401, "Refresh token is required."));
        }

        try
        {
            var result = await _authService.RefreshTokenAsync(refreshToken, ClientIpAddress);
            SetRefreshTokenCookie(result.RefreshTokenValue);
            return Ok(ApiResponseDto<AuthResponseDto>.Ok(result));
        }
        catch (UnauthorizedException)
        {
            DeleteRefreshTokenCookie();
            throw;
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto? request = null)
    {
        if (!IsOriginAllowed())
            return StatusCode(403, ApiResponseDto<object>.Fail(403, "Request origin is not allowed."));

        var refreshToken = request?.RefreshToken;
        if (string.IsNullOrWhiteSpace(refreshToken))
            HttpContext.Request.Cookies.TryGetValue("eventnest.refresh_token", out refreshToken);

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            DeleteRefreshTokenCookie();
            return NoContent();
        }

        await _authService.LogoutAsync(refreshToken, ClientIpAddress);
        DeleteRefreshTokenCookie();
        return NoContent();
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append("eventnest.refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = _options.CookieSecure,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
            MaxAge = TimeSpan.FromDays(30)
        });
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete("eventnest.refresh_token", new CookieOptions
        {
            Path = "/api/auth",
            SameSite = SameSiteMode.Lax
        });
    }
}
