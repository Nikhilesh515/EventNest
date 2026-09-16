using EventNest.AuthService.Application.DTOs.Users;

namespace EventNest.AuthService.Application.Services.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid userId);
    Task<PagedResultDto<UserDto>> GetPagedAsync(int page, int pageSize, string? search, Guid? roleId);
    Task<UserDto> CreateAsync(CreateUserRequestDto request);
    Task<UserDto> UpdateAsync(Guid userId, string displayName);
    Task DeactivateAsync(Guid userId);
}
