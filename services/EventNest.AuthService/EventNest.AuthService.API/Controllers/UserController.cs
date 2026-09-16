using System.Security.Claims;
using EventNest.Shared.Application.Authorization;
using EventNest.Shared.Application.DTOs;
using EventNest.Shared.Domain.Exceptions;
using EventNest.AuthService.Application.DTOs.Users;
using EventNest.AuthService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventNest.AuthService.API.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            throw new UnauthorizedException("Invalid token.");

        var user = await _userService.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException($"User with ID '{id}' was not found.");

        return Ok(ApiResponseDto<UserDto>.Ok(user));
    }

    [HttpGet("{id:guid}")]
    [Authorize(EventNestPermissions.Users.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null)
            return NotFound(ApiResponseDto<UserDto>.Fail(404, $"User with ID '{id}' was not found."));

        return Ok(ApiResponseDto<UserDto>.Ok(user));
    }

    [HttpGet]
    [Authorize(EventNestPermissions.Users.View)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var users = await _userService.GetAllAsync(page, pageSize);
        return Ok(ApiResponseDto<IReadOnlyList<UserDto>>.Ok(users));
    }

    [HttpPut("{id:guid}")]
    [Authorize(EventNestPermissions.Users.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequestDto request)
    {
        var user = await _userService.UpdateAsync(id, request.DisplayName);
        return Ok(ApiResponseDto<UserDto>.Ok(user));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(EventNestPermissions.Users.Manage)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _userService.DeactivateAsync(id);
        return NoContent();
    }
}
