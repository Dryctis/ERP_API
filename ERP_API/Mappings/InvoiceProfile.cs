using AutoMapper;
using ERP_API.DTOs;
using ERP_API.Entities;

namespace ERP_API.Mappings;


public class InvoiceProfile : Profile
{
    public InvoiceProfile()
    {
       

        CreateMap<InvoicePayment, InvoicePaymentDto>()
            .ForMember(dest => dest.PaymentMethod,
                opt => opt.MapFrom(src => src.PaymentMethod.ToString()));

        CreateMap<InvoicePaymentCreateDto, InvoicePayment>()
            .ForMember(dest => dest.PaymentMethod,
                opt => opt.MapFrom(src => (PaymentMethod)src.PaymentMethod))
            .ForMember(dest => dest.PaymentDate,
                opt => opt.MapFrom(src => src.PaymentDate ?? DateTime.UtcNow));
    }
}