using EventNest.AuthService.Application.DTOs.Users;

namespace EventNest.AuthService.Application.Services.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid userId);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<IReadOnlyList<UserDto>> GetAllAsync(int page, int pageSize);
    Task<UserDto> UpdateAsync(Guid userId, string displayName);
    Task DeactivateAsync(Guid userId);
}
