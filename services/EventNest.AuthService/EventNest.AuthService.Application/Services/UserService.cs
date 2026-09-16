using EventNest.AuthService.Application.DTOs.Users;
using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Application.Services.Interfaces;
using EventNest.Shared.Domain.Exceptions;

namespace EventNest.AuthService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public UserService(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<UserDto?> GetByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return null;

        var role = await _roleRepository.GetByIdAsync(user.RoleId);
        return new UserDto(user.Id, user.Email, user.DisplayName, role?.Name ?? "User", user.IsActive);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(int page, int pageSize)
    {
        var users = await _userRepository.GetAllAsync(page, pageSize);
        var result = new List<UserDto>();

        foreach (var user in users)
        {
            var role = await _roleRepository.GetByIdAsync(user.RoleId);
            result.Add(new UserDto(user.Id, user.Email, user.DisplayName, role?.Name ?? "User", user.IsActive));
        }

        return result;
    }

    public async Task<UserDto> UpdateAsync(Guid userId, string displayName)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException($"User with ID '{userId}' was not found.");

        user.UpdateDisplayName(displayName);
        await _userRepository.UpdateAsync(user);

        var role = await _roleRepository.GetByIdAsync(user.RoleId);
        return new UserDto(user.Id, user.Email, user.DisplayName, role?.Name ?? "User", user.IsActive);
    }

    public async Task DeactivateAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException($"User with ID '{userId}' was not found.");

        user.Deactivate();
        await _userRepository.UpdateAsync(user);
    }
}
