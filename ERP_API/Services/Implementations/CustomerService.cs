using AutoMapper;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;

namespace ERP_API.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repo;
    private readonly IMapper _mapper;
    public CustomerService(ICustomerRepository repo, IMapper mapper)
    {
        _repo = repo; _mapper = mapper;
    }

    public async Task<object> GetPagedAsync(int page, int pageSize, string? q, string? sort)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, q, sort);
        var result = _mapper.Map<List<CustomerDto>>(items);
        return new { total, page, pageSize, items = result };
    }

    public async Task<CustomerDto?> GetAsync(Guid id)
    {
        var c = await _repo.GetByIdAsync(id);
        return c is null ? null : _mapper.Map<CustomerDto>(c);
    }

    public async Task<(bool ok, string? error, CustomerDto? dto)> CreateAsync(CustomerCreateDto dto)
    {
        if (await _repo.ExistsByEmailAsync(dto.Email)) return (false, "Email already exists", null);
        var c = _mapper.Map<Customer>(dto);
        await _repo.AddAsync(c);
        await _repo.SaveAsync();
        return (true, null, _mapper.Map<CustomerDto>(c));
    }

    public async Task<(bool ok, string? error, CustomerDto? dto)> UpdateAsync(Guid id, CustomerUpdateDto dto)
    {
        var c = await _repo.GetByIdAsync(id);
        if (c is null) return (false, "NotFound", null);
        if (await _repo.ExistsByEmailAsync(dto.Email, id)) return (false, "Email already exists", null);
        _mapper.Map(dto, c);
        await _repo.UpdateAsync(c);
        await _repo.SaveAsync();
        return (true, null, _mapper.Map<CustomerDto>(c));
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var c = await _repo.GetByIdAsync(id);
        if (c is null) return false;
        await _repo.DeleteAsync(c);
        await _repo.SaveAsync();
        return true;
    }
}
