using AutoMapper;
using ERP_API.DTOs;
using ERP_API.Entities;

namespace ERP_API.Mappings
{
    public class InventoryMappingProfile : Profile
    {
        public InventoryMappingProfile()
        {
            
            CreateMap<InventoryMovement, InventoryMovementDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.MovementType, opt => opt.MapFrom(src => (int)src.MovementType));

            
            CreateMap<InventoryMovementCreateDto, InventoryMovement>()
                .ForMember(dest => dest.MovementType, opt => opt.MapFrom(src => (MovementType)src.MovementType));
        }
    }
}
