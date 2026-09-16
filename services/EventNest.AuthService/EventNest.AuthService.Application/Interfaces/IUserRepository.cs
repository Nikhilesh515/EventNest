using EventNest.AuthService.Domain.Entities;

namespace EventNest.AuthService.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<(IReadOnlyList<User> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search, Guid? roleId);
    Task<int> CountByRoleIdAsync(Guid roleId);
    Task<int> CountActiveByRoleIdAsync(Guid roleId);
    Task<IReadOnlyList<Guid>> GetUserIdsByRoleIdAsync(Guid roleId);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}
