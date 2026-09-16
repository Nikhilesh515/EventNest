using EventNest.AuthService.Application.DTOs.Users;
using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Application.Services.Interfaces;
using EventNest.AuthService.Domain.Entities;
using EventNest.Shared.Domain.Exceptions;

namespace EventNest.AuthService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto?> GetByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return null;

        var role = await _roleRepository.GetByIdAsync(user.RoleId);
        return new UserDto(user.Id, user.Email, user.DisplayName, role?.Name ?? "User", user.RoleId, user.IsActive);
    }

    public async Task<PagedResultDto<UserDto>> GetPagedAsync(int page, int pageSize, string? search, Guid? roleId)
    {
        var errors = new Dictionary<string, string[]>();

        if (page < 1)
            errors["page"] = new[] { "Page must be at least 1." };

        if (pageSize < 1 || pageSize > 100)
            errors["pageSize"] = new[] { "Page size must be between 1 and 100." };

        if (errors.Count > 0)
            throw new ValidationException(errors);

        var (users, total) = await _userRepository.GetPagedAsync(page, pageSize, search, roleId);

        var roleNames = new Dictionary<Guid, string>();
        var items = new List<UserDto>();

        foreach (var user in users)
        {
            if (!roleNames.TryGetValue(user.RoleId, out var roleName))
            {
                var role = await _roleRepository.GetByIdAsync(user.RoleId);
                roleName = role?.Name ?? "User";
                roleNames[user.RoleId] = roleName;
            }

            items.Add(new UserDto(user.Id, user.Email, user.DisplayName, roleName, user.RoleId, user.IsActive));
        }

        var pages = (int)Math.Ceiling((double)total / pageSize);
        return new PagedResultDto<UserDto>(items, total, page, pageSize, pages);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Email))
            errors["email"] = new[] { "Email is required." };

        if (string.IsNullOrWhiteSpace(request.DisplayName))
            errors["displayName"] = new[] { "Display name is required." };

        if (string.IsNullOrWhiteSpace(request.Password))
            errors["password"] = new[] { "Password is required." };

        if (errors.Count > 0)
            throw new ValidationException(errors);

        var email = request.Email.Trim();

        if (await _userRepository.GetByEmailAsync(email) is not null)
            throw new ConflictException($"User with email '{email}' is already registered.");

        var role = await _roleRepository.GetByIdAsync(request.RoleId)
            ?? throw new NotFoundException($"Role with ID '{request.RoleId}' was not found.");

        var user = User.Create(email, request.DisplayName.Trim(), _passwordHasher.Hash(request.Password), role.Id);
        await _userRepository.AddAsync(user);

        return new UserDto(user.Id, user.Email, user.DisplayName, role.Name, role.Id, user.IsActive);
    }

    public async Task<UserDto> UpdateAsync(Guid userId, string displayName)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException($"User with ID '{userId}' was not found.");

        user.UpdateDisplayName(displayName);
        await _userRepository.UpdateAsync(user);

        var role = await _roleRepository.GetByIdAsync(user.RoleId);
        return new UserDto(user.Id, user.Email, user.DisplayName, role?.Name ?? "User", user.RoleId, user.IsActive);
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
