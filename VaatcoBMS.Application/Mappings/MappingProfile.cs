using VaatcoBMS.Application.DTOs.Customer;
using VaatcoBMS.Application.DTOs.Invoice;
using VaatcoBMS.Application.DTOs.InvoiceItem;
using VaatcoBMS.Application.DTOs.Product;
using VaatcoBMS.Application.DTOs.User;
using VaatcoBMS.Application.Model.Auth;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Enums;

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
    // Custom mapping for Product to ProductDto with specific field mappings
    CreateMap<Product, ProductDto>()
          .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Code))
          .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Name))
          .ForMember(dest => dest.StockStatus, opt => opt.MapFrom(src => MapStockStatus(src.StockStatus)));
  }

		 // Helper method to keep the mapping clean
		private static string MapStockStatus(StockStatus status)
   	{
		return status switch
		{
			StockStatus.InStock => "OK",
			StockStatus.LowStock => "Low",
			StockStatus.OutOfStock => "Out of Stock",
			StockStatus.Discontinued => "Discontinued",
			_ => "Unknown"
		};
	}
}

