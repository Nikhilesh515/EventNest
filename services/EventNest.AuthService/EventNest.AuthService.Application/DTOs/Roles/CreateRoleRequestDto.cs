namespace EventNest.AuthService.Application.DTOs.Roles;

public record CreateRoleRequestDto(
    string Name,
    string DisplayName,
    string? Description,
    int? SortOrder,
    List<string> PermissionNames);
