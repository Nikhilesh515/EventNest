using EventNest.TagService.Application.DTOs.Tags;

namespace EventNest.TagService.Application.Services.Interfaces;

public interface ITagService
{
    Task<TagDto> CreateAsync(CreateTagRequestDto request);
    Task<TagDto?> GetByIdAsync(Guid id);
    Task<List<TagDto>> GetAllAsync();
    Task<TagDto> UpdateAsync(Guid id, UpdateTagRequestDto request);
    Task DeleteAsync(Guid id);
}
