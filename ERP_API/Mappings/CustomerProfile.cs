using AutoMapper;
using ERP_API.DTOs;
using ERP_API.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ERP_API.Mappings;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDto>();
        CreateMap<CustomerCreateDto, Customer>();
        CreateMap<CustomerUpdateDto, Customer>();
    }
}
