using EventNest.AuthService.Application.DTOs.Roles;
using EventNest.AuthService.Application.DTOs.Users;
using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Application.Services.Interfaces;
using EventNest.AuthService.Domain.Entities;
using EventNest.Shared.Application.Authorization;
using EventNest.Shared.Domain.Exceptions;

namespace EventNest.AuthService.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPermissionStore _permissionStore;

    public RoleService(
        IRoleRepository roleRepository,
        IRolePermissionRepository rolePermissionRepository,
        IUserRepository userRepository,
        IPermissionStore permissionStore)
    {
        _roleRepository = roleRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _userRepository = userRepository;
        _permissionStore = permissionStore;
    }

    public async Task<List<RoleDto>> GetAllAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        var result = new List<RoleDto>();

        foreach (var role in roles)
            result.Add(await MapAsync(role));

        return result;
    }

    public async Task<RoleDto?> GetByIdAsync(Guid id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        return role is null ? null : await MapAsync(role);
    }

    public async Task<RoleDto> CreateAsync(CreateRoleRequestDto request)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["name"] = new[] { "Role name is required." }
            });

        if (await _roleRepository.GetByNameAsync(name) is not null)
            throw new ConflictException($"Role with name '{name}' already exists.");

        var permissions = ValidatePermissions(request.PermissionNames);
        var displayName = string.IsNullOrWhiteSpace(request.DisplayName) ? name : request.DisplayName.Trim();

        var role = Role.Create(name, displayName, request.Description, request.SortOrder ?? 0);
        await _roleRepository.AddAsync(role);

        if (permissions.Count > 0)
            await _rolePermissionRepository.AddRangeAsync(
                permissions.Select(p => RolePermission.Create(role.Id, p)));

        return await MapAsync(role);
    }

    public async Task<RoleDto> UpdateAsync(Guid id, UpdateRoleRequestDto request)
    {
        var role = await _roleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Role with ID '{id}' was not found.");

        var permissions = ValidatePermissions(request.PermissionNames);
        var displayName = string.IsNullOrWhiteSpace(request.DisplayName) ? role.DisplayName : request.DisplayName.Trim();

        role.UpdateDetails(displayName, request.Description, request.SortOrder ?? role.SortOrder);
        await _roleRepository.UpdateAsync(role);

        await _rolePermissionRepository.RemoveByRoleIdAsync(role.Id);

        if (permissions.Count > 0)
            await _rolePermissionRepository.AddRangeAsync(
                permissions.Select(p => RolePermission.Create(role.Id, p)));

        await _permissionStore.InvalidateRoleAsync(role.Id);

        return await MapAsync(role);
    }

    public async Task DeleteAsync(Guid id)
    {
        var role = await _roleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Role with ID '{id}' was not found.");

        if (EventNestPermissions.IsBuiltInRole(role.Name))
            throw new ConflictException($"Built-in role '{role.Name}' cannot be deleted.");

        var assignedUsers = await _userRepository.CountByRoleIdAsync(role.Id);
        if (assignedUsers > 0)
            throw new ConflictException($"Role '{role.Name}' still has {assignedUsers} user(s) assigned.");

        await _roleRepository.DeleteAsync(role);
    }

    public async Task<UserDto> AssignRoleAsync(Guid userId, Guid roleId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User with ID '{userId}' was not found.");

        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException($"Role with ID '{roleId}' was not found.");

        user.ChangeRole(role.Id);
        await _userRepository.UpdateAsync(user);
        await _permissionStore.InvalidateUserAsync(userId);

        return new UserDto(user.Id, user.Email, user.DisplayName, role.Name, role.Id, user.IsActive);
    }

    private async Task<RoleDto> MapAsync(Role role)
    {
        var permissions = await _rolePermissionRepository.GetByRoleIdAsync(role.Id);
        var userCount = await _userRepository.CountActiveByRoleIdAsync(role.Id);

        return new RoleDto(
            role.Id,
            role.Name,
            role.DisplayName,
            role.Description,
            role.SortOrder,
            userCount,
            permissions.Select(p => p.PermissionName).OrderBy(p => p).ToList());
    }

    private static List<string> ValidatePermissions(List<string>? permissionNames)
    {
        if (permissionNames is null || permissionNames.Count == 0)
            return new List<string>();

        var distinct = permissionNames.Distinct().ToList();
        var invalid = distinct.Where(p => !EventNestPermissions.IsValid(p)).ToList();

        if (invalid.Count > 0)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["permissionNames"] = new[] { $"Unknown permission(s): {string.Join(", ", invalid)}" }
            });

        return distinct;
    }
}
