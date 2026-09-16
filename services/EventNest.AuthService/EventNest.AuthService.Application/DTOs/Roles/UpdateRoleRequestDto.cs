namespace EventNest.AuthService.Application.DTOs.Roles;

public record UpdateRoleRequestDto(
    string DisplayName,
    string? Description,
    int? SortOrder,
    List<string> PermissionNames);
