using AutoMapper;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;

namespace ERP_API.Services.Implementations;


public class CustomerService : ICustomerService
{
    private readonly IUnidadDeTrabajo _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerService(IUnidadDeTrabajo unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<object> GetPagedAsync(int page, int pageSize, string? q, string? sort)
    {
        var (items, total) = await _unitOfWork.Customers.GetPagedAsync(page, pageSize, q, sort);
        var result = _mapper.Map<List<CustomerDto>>(items);
        return new { total, page, pageSize, items = result };
    }

    public async Task<CustomerDto?> GetAsync(Guid id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        return customer is null ? null : _mapper.Map<CustomerDto>(customer);
    }

    public async Task<(bool ok, string? error, CustomerDto? dto)> CreateAsync(CustomerCreateDto dto)
    {
        
        if (await _unitOfWork.Customers.ExistsByEmailAsync(dto.Email))
            return (false, "Email already exists", null);

       
        var customer = _mapper.Map<Customer>(dto);
        await _unitOfWork.Customers.AddAsync(customer);

        
        await _unitOfWork.SaveChangesAsync();

        return (true, null, _mapper.Map<CustomerDto>(customer));
    }

    public async Task<(bool ok, string? error, CustomerDto? dto)> UpdateAsync(Guid id, CustomerUpdateDto dto)
    {
        
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer is null)
            return (false, "NotFound", null);

       
        if (await _unitOfWork.Customers.ExistsByEmailAsync(dto.Email, id))
            return (false, "Email already exists", null);

        
        _mapper.Map(dto, customer);
        await _unitOfWork.Customers.UpdateAsync(customer);

       
        await _unitOfWork.SaveChangesAsync();

        return (true, null, _mapper.Map<CustomerDto>(customer));
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer is null)
            return false;

        await _unitOfWork.Customers.DeleteAsync(customer);

        
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}