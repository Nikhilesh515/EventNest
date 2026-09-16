using EventNest.AuthService.Application.Interfaces;
using EventNest.AuthService.Domain.Entities;
using EventNest.AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventNest.AuthService.Infrastructure.Data.Repositories;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly AuthDbContext _context;

    public RolePermissionRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RolePermission>> GetByRoleIdAsync(Guid roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<RolePermission> permissions)
    {
        await _context.RolePermissions.AddRangeAsync(permissions);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveByRoleIdAsync(Guid roleId)
    {
        await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ExecuteDeleteAsync();
    }
}
