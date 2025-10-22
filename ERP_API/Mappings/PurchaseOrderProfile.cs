using AutoMapper;
using ERP_API.DTOs;
using ERP_API.Entities;

namespace ERP_API.Mappings;

public class PurchaseOrderProfile : Profile
{
    public PurchaseOrderProfile()
    {

        CreateMap<PurchaseOrderItemCreateDto, PurchaseOrderItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseOrderId, opt => opt.Ignore())
            .ForMember(dest => dest.ReceivedQuantity, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.Subtotal, opt => opt.Ignore())
            .ForMember(dest => dest.TaxAmount, opt => opt.Ignore())
            .ForMember(dest => dest.LineTotal, opt => opt.Ignore())
            .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}