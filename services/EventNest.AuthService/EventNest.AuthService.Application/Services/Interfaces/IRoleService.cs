using EventNest.AuthService.Application.DTOs.Roles;
using EventNest.AuthService.Application.DTOs.Users;

namespace EventNest.AuthService.Application.Services.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllAsync();
    Task<RoleDto?> GetByIdAsync(Guid id);
    Task<RoleDto> CreateAsync(CreateRoleRequestDto request);
    Task<RoleDto> UpdateAsync(Guid id, UpdateRoleRequestDto request);
    Task DeleteAsync(Guid id);
    Task<UserDto> AssignRoleAsync(Guid userId, Guid roleId);
}
