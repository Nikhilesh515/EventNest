using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventNest.AuthService.Infrastructure.Data.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AuthDbContext _context;

    public RoleRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<Role?> GetByIdAsync(Guid id)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync()
    {
        return await _context.Roles
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.Name)
            .ToListAsync();
    }

    public async Task AddAsync(Role role)
    {
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Role role)
    {
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Role role)
    {
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
    }
}
