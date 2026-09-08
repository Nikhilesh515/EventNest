namespace EventNest.AuthService.Application.DTOs;

public record PermissionDto(string Name, string DisplayName, string Group, bool IsGranted);
