using ERP_API.Entities;

namespace ERP_API.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<(IReadOnlyList<Customer> items, int total)> GetPagedAsync(int page, int pageSize, string? q, string? sort);
    Task<Customer?> GetByIdAsync(Guid id);
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null);
    Task AddAsync(Customer c);
    Task UpdateAsync(Customer c);
    Task DeleteAsync(Customer c);
    //Task SaveAsync();
}
