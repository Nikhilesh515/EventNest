using EventNest.TagService.Domain.Entities;

namespace EventNest.TagService.Application.Services.Interfaces;

public interface ITagRepository
{
    Task<Tag> AddAsync(Tag tag);
    Task<Tag?> GetByIdAsync(Guid id);
    Task<List<Tag>> GetAllAsync();
    Task<Tag?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
    Task UpdateAsync(Tag tag);
    Task DeleteAsync(Tag tag);
    Task<int> SaveChangesAsync();
}
