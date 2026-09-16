namespace EventNest.AuthService.Application.DTOs.Users;

public record PagedResultDto<T>(List<T> Items, int Total, int Page, int Size, int Pages);
