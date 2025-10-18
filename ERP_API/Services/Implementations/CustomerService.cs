using AutoMapper;
using ERP_API.Common.Results;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;

namespace ERP_API.Services.Implementations;


public class CustomerService : ICustomerService
{
    private readonly IUnidadDeTrabajo _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(IUnidadDeTrabajo unitOfWork, IMapper mapper, ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<object> GetPagedAsync(int page, int pageSize, string? q, string? sort)
    {
        _logger.LogDebug(
            "Consultando clientes paginados. Página: {Page}, Tamaño: {PageSize}, Búsqueda: {Query}, Orden: {Sort}",
            page, pageSize, q ?? "ninguna", sort ?? "name:asc"
        );

        var (items, total) = await _unitOfWork.Customers.GetPagedAsync(page, pageSize, q, sort);
        var result = _mapper.Map<List<CustomerDto>>(items);

        _logger.LogInformation(
            "Clientes obtenidos. Total: {Total}, Página: {Page}, Resultados: {Count}",
            total, page, result.Count
        );

        return new { total, page, pageSize, items = result };
    }

    public async Task<Result<CustomerDto>> GetAsync(Guid id)
    {
        _logger.LogDebug("Consultando cliente. CustomerId: {CustomerId}", id);

        var customer = await _unitOfWork.Customers.GetByIdAsync(id);

        if (customer is null)
        {
            _logger.LogWarning("Cliente no encontrado. CustomerId: {CustomerId}", id);
            return Result<CustomerDto>.Failure("Customer not found");
        }

        _logger.LogDebug("Cliente encontrado. CustomerId: {CustomerId}, Nombre: {Name}", id, customer.Name);
        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<Result<CustomerDto>> CreateAsync(CustomerCreateDto dto)
    {
        _logger.LogInformation(
            "Iniciando creación de cliente. Email: {Email}, Nombre: {Name}",
            dto.Email, dto.Name
        );

        
        if (await _unitOfWork.Customers.ExistsByEmailAsync(dto.Email))
        {
            _logger.LogWarning("Intento de crear cliente con email duplicado. Email: {Email}", dto.Email);
            return Result<CustomerDto>.Failure("Email already exists");
        }

        
        var customer = _mapper.Map<Customer>(dto);
        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Cliente creado exitosamente. CustomerId: {CustomerId}, Email: {Email}, Nombre: {Name}",
            customer.Id, customer.Email, customer.Name
        );

        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<Result<CustomerDto>> UpdateAsync(Guid id, CustomerUpdateDto dto)
    {
        _logger.LogInformation(
            "Iniciando actualización de cliente. CustomerId: {CustomerId}, Email: {Email}",
            id, dto.Email
        );

        
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer is null)
        {
            _logger.LogWarning("Cliente no encontrado para actualizar. CustomerId: {CustomerId}", id);
            return Result<CustomerDto>.Failure("Customer not found");
        }

        var nombreAnterior = customer.Name;
        var emailAnterior = customer.Email;

        
        if (await _unitOfWork.Customers.ExistsByEmailAsync(dto.Email, id))
        {
            _logger.LogWarning(
                "Intento de actualizar cliente con email duplicado. CustomerId: {CustomerId}, Email: {Email}",
                id, dto.Email
            );
            return Result<CustomerDto>.Failure("Email already exists");
        }

       
        _mapper.Map(dto, customer);
        await _unitOfWork.Customers.UpdateAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Cliente actualizado exitosamente. CustomerId: {CustomerId}, Nombre: {NombreAnterior} → {NombreNuevo}, Email: {EmailAnterior} → {EmailNuevo}",
            customer.Id, nombreAnterior, customer.Name, emailAnterior, customer.Email
        );

        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Iniciando eliminación de cliente. CustomerId: {CustomerId}", id);

        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer is null)
        {
            _logger.LogWarning("Cliente no encontrado para eliminar. CustomerId: {CustomerId}", id);
            return Result.Failure("Customer not found");
        }

        var nombre = customer.Name;
        var email = customer.Email;

        await _unitOfWork.Customers.DeleteAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning(
            "Cliente eliminado. CustomerId: {CustomerId}, Nombre: {Name}, Email: {Email}",
            id, nombre, email
        );

        return Result.Success();
    }
}