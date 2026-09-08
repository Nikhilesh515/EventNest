using EventNest.AuthService.Application.DTOs.Auth;
using EventNest.AuthService.Application.DTOs.Common;
using EventNest.AuthService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventNest.AuthService.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request.Email, request.DisplayName, request.Password);
        return Ok(ApiResponseDto<AuthResponseDto>.Ok(result));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);
        return Ok(ApiResponseDto<AuthResponseDto>.Ok(result));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(ApiResponseDto<AuthResponseDto>.Ok(result));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return NoContent();
    }
}
