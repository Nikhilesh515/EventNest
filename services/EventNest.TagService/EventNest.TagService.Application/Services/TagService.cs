using EventNest.TagService.Application.DTOs.Tags;
using EventNest.TagService.Application.Services.Interfaces;
using EventNest.TagService.Domain.Entities;
using EventNest.Shared.Domain.Exceptions;

namespace EventNest.TagService.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _repository;

    public TagService(ITagRepository repository)
    {
        _repository = repository;
    }

    public async Task<TagDto> CreateAsync(CreateTagRequestDto request)
    {
        if (await _repository.ExistsByNameAsync(request.Name))
            throw new ConflictException($"Tag with name '{request.Name}' already exists.");

        var tag = Tag.Create(request.Name, request.Color);
        await _repository.AddAsync(tag);
        await _repository.SaveChangesAsync();

        return new TagDto(tag.Id, tag.Name, tag.Color, tag.CreatedAt);
    }

    public async Task<TagDto?> GetByIdAsync(Guid id)
    {
        var tag = await _repository.GetByIdAsync(id);
        if (tag is null) return null;
        return new TagDto(tag.Id, tag.Name, tag.Color, tag.CreatedAt);
    }

    public async Task<List<TagDto>> GetAllAsync()
    {
        var tags = await _repository.GetAllAsync();
        return tags.Select(t => new TagDto(t.Id, t.Name, t.Color, t.CreatedAt)).ToList();
    }

    public async Task<TagDto> UpdateAsync(Guid id, UpdateTagRequestDto request)
    {
        var tag = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Tag with id {id} not found.");

        if (await _repository.ExistsByNameAsync(request.Name, id))
            throw new ConflictException($"Tag with name '{request.Name}' already exists.");

        tag.UpdateName(request.Name);
        tag.UpdateColor(request.Color);
        await _repository.UpdateAsync(tag);
        await _repository.SaveChangesAsync();

        return new TagDto(tag.Id, tag.Name, tag.Color, tag.CreatedAt);
    }

    public async Task DeleteAsync(Guid id)
    {
        var tag = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Tag with id {id} not found.");

        await _repository.DeleteAsync(tag);
        await _repository.SaveChangesAsync();
    }
}
