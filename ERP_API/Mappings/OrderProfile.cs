using AutoMapper;
using ERP_API.DTOs;
using ERP_API.Entities;

namespace ERP_API.Mappings;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name));
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));
    }
}
