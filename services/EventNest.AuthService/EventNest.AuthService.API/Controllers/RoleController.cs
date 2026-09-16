using EventNest.Shared.Application.Authorization;
using EventNest.Shared.Application.DTOs;
using EventNest.AuthService.Application.DTOs.Roles;
using EventNest.AuthService.Application.DTOs.Users;
using EventNest.AuthService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventNest.AuthService.API.Controllers;

[ApiController]
[Route("api/roles")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [Authorize(EventNestPermissions.Users.View)]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleService.GetAllAsync();
        return Ok(ApiResponseDto<List<RoleDto>>.Ok(roles));
    }

    [HttpGet("{id:guid}")]
    [Authorize(EventNestPermissions.Users.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var role = await _roleService.GetByIdAsync(id);
        if (role is null)
            return NotFound(ApiResponseDto<RoleDto>.Fail(404, $"Role with ID '{id}' was not found."));

        return Ok(ApiResponseDto<RoleDto>.Ok(role));
    }

    [HttpPost]
    [Authorize(EventNestPermissions.Users.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequestDto request)
    {
        var result = await _roleService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponseDto<RoleDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(EventNestPermissions.Users.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequestDto request)
    {
        var result = await _roleService.UpdateAsync(id, request);
        return Ok(ApiResponseDto<RoleDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(EventNestPermissions.Users.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _roleService.DeleteAsync(id);
        return NoContent();
    }
}
