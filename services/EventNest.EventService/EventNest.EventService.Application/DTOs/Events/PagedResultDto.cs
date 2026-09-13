namespace EventNest.EventService.Application.DTOs.Events;

public record PagedResultDto<T>(
    List<T> Items,
    int Total,
    int Page,
    int Size,
    int Pages);
