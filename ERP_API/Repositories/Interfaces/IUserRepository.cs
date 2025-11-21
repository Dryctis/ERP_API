using ERP_API.Entities;

namespace ERP_API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<(IReadOnlyList<User> items, int total)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null,
        string? role = null,
        string? sort = null);

    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByIdWithRolesAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null);
    Task<IReadOnlyList<User>> GetByRoleAsync(string roleName);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
}