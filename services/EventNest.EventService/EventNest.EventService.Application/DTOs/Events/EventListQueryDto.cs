namespace EventNest.EventService.Application.DTOs.Events;

public record EventListQueryDto(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    List<Guid>? TagId = null,
    string? Visibility = null,
    string? Status = null,
    string? Timeframe = null,
    string? Sort = null);
