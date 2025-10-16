using AutoMapper;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;

namespace ERP_API.Services.Implementations;


public class ProductService : IProductService
{
    private readonly IUnidadDeTrabajo _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(IUnidadDeTrabajo unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<object> GetPagedAsync(int page, int pageSize, string? q, string? sort)
    {
        var (items, total) = await _unitOfWork.Products.GetPagedAsync(page, pageSize, q, sort);
        var result = _mapper.Map<List<ProductDto>>(items);
        return new { total, page, pageSize, items = result };
    }

    public async Task<ProductDto?> GetAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        return product is null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(ProductCreateDto dto)
    {
        var product = _mapper.Map<Product>(dto);

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, ProductUpdateDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null)
            return null;

        
        _mapper.Map(dto, product);

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null)
            return false;

        await _unitOfWork.Products.DeleteAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}