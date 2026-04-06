using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Application.DTOs;
using VaatcoBMS.Application.Model.Auth;

namespace VaatcoBMS.Application.Mappings;

public class MappingProfile : AutoMapper.Profile
{
    public MappingProfile()
    {
        // Simple direct mappings
        CreateMap<Customer, CustomerDto>().ReverseMap();
        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<InvoiceItem, InvoiceItemDto>().ReverseMap();

        // Invoice mapping with related entity names
        CreateMap<Invoice, InvoiceDto>()
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null ? s.Customer.Name : string.Empty))
            // FIX: Use the navigation property 'CreatorUser' instead of the integer ID 'CreatedBy'
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatorUser != null ? s.CreatorUser.Name : string.Empty))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        // Auth Mappings
        CreateMap<Register, User>()
            .ForMember(d => d.PasswordHash, o => o.Ignore()) // Do not map plain text password to hash
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role));
    }
}
