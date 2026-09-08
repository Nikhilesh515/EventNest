using EventNest.AuthService.Application.DTOs;
using EventNest.AuthService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventNest.AuthService.API.Controllers;

[ApiController]
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
        // In a real implementation, get user ID from JWT claims
        // For now, return a placeholder
        return Ok(ApiResponse<object>.Fail(401, "Authentication required."));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null)
            return NotFound(ApiResponse<UserDto>.Fail(404, $"User with ID '{id}' was not found."));

        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var users = await _userService.GetAllAsync(page, pageSize);
        return Ok(ApiResponse<IReadOnlyList<UserDto>>.Ok(users));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userService.UpdateAsync(id, request.DisplayName);
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _userService.DeactivateAsync(id);
        return NoContent();
    }
}

public record UpdateUserRequest(string DisplayName);
