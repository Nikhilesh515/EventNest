namespace EventNest.AuthService.Application.DTOs.Roles;

public record RoleDto(
    Guid Id,
    string Name,
    string DisplayName,
    string? Description,
    int SortOrder,
    int UserCount,
    List<string> PermissionNames);
