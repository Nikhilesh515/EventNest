using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventNest.AuthService.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<(IReadOnlyList<User> Items, int Total)> GetPagedAsync(
        int page, int pageSize, string? search, Guid? roleId)
    {
        var query = _context.Users.Where(u => u.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u => EF.Functions.ILike(u.Email, $"%{term}%")
                                  || EF.Functions.ILike(u.DisplayName, $"%{term}%"));
        }

        if (roleId.HasValue)
            query = query.Where(u => u.RoleId == roleId.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<int> CountByRoleIdAsync(Guid roleId)
    {
        return await _context.Users.CountAsync(u => u.RoleId == roleId);
    }

    public async Task<int> CountActiveByRoleIdAsync(Guid roleId)
    {
        return await _context.Users.CountAsync(u => u.RoleId == roleId && u.IsActive);
    }

    public async Task<IReadOnlyList<Guid>> GetUserIdsByRoleIdAsync(Guid roleId)
    {
        return await _context.Users
            .Where(u => u.RoleId == roleId)
            .Select(u => u.Id)
            .ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}
