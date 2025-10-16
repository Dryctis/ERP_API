using AutoMapper;
using ERP_API.Entities;
using ERP_API.DTOs;

namespace ERP_API.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        
        CreateMap<Product, ProductDto>();

      
        CreateMap<ProductCreateDto, Product>();
        CreateMap<ProductUpdateDto, Product>();
    }
}