using AutoMapper;
using ERP_API.DTOs;
using ERP_API.Entities;

namespace ERP_API.Mappings;

public class SupplierProfile : Profile
{
    public SupplierProfile()
    {
     
        CreateMap<Supplier, SupplierDto>()
            .ForMember(dest => dest.ProductCount,
                opt => opt.MapFrom(src => src.ProductSuppliers.Count));

        CreateMap<SupplierCreateDto, Supplier>();

        CreateMap<SupplierUpdateDto, Supplier>();

        
        CreateMap<ProductSupplierCreateDto, ProductSupplier>();

        CreateMap<ProductSupplierUpdateDto, ProductSupplier>();

        CreateMap<ProductSupplier, ProductSupplierDto>()
            .ForMember(dest => dest.ProductName,
                opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductSku,
                opt => opt.MapFrom(src => src.Product.Sku))
            .ForMember(dest => dest.SupplierName,
                opt => opt.MapFrom(src => src.Supplier.Name));
    }
}