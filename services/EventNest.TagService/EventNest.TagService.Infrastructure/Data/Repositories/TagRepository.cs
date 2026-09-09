using EventNest.TagService.Application.Services.Interfaces;
using EventNest.TagService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventNest.TagService.Infrastructure.Data.Repositories;

public class TagRepository : ITagRepository
{
    private readonly TagDbContext _context;

    public TagRepository(TagDbContext context)
    {
        _context = context;
    }

    public async Task<Tag> AddAsync(Tag tag)
    {
        await _context.Tags.AddAsync(tag);
        return tag;
    }

    public async Task<Tag?> GetByIdAsync(Guid id)
    {
        return await _context.Tags.FindAsync(id);
    }

    public async Task<List<Tag>> GetAllAsync()
    {
        return await _context.Tags.ToListAsync();
    }

    public async Task<Tag?> GetByNameAsync(string name)
    {
        return await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
    {
        return await _context.Tags.AnyAsync(t =>
            t.Name == name && (excludeId == null || t.Id != excludeId));
    }

    public async Task UpdateAsync(Tag tag)
    {
        _context.Tags.Update(tag);
    }

    public async Task DeleteAsync(Tag tag)
    {
        _context.Tags.Remove(tag);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
