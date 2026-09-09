namespace EventNest.TagService.Application.DTOs.Tags;

public record TagDto(Guid Id, string Name, string Color, DateTime CreatedAt);
