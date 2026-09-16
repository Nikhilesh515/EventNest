using EventNest.AuthService.Domain.Entities;

namespace EventNest.AuthService.Application.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name);
    Task<Role?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Role>> GetAllAsync();
    Task AddAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(Role role);
}
