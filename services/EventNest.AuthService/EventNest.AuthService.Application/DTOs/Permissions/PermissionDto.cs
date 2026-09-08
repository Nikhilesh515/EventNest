namespace EventNest.AuthService.Application.DTOs.Permissions;

public record PermissionDto(string Name, string DisplayName, string Group, bool IsGranted);
