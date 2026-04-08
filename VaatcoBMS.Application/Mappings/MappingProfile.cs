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
		// ── CUSTOMER MAPPINGS ──
		CreateMap<Customer, CustomerDto>()
			.ForMember(d => d.TotalInvoices, o => o.MapFrom(s => s.Invoices != null ? s.Invoices.Count : 0));
		CreateMap<CreateCustomerDto, Customer>();
		CreateMap<UpdateCustomerDto, Customer>();

		// ── PRODUCT MAPPINGS ──
		CreateMap<Product, ProductDto>()
			.ForMember(d => d.ProductCode, opt => opt.MapFrom(src => src.Code))
			.ForMember(d => d.ProductName, opt => opt.MapFrom(src => src.Name))
			.ForMember(d => d.StockStatus, opt => opt.MapFrom(src => MapStockStatus(src.StockStatus)));

		CreateMap<CreateProductDto, Product>();
		CreateMap<UpdateProductDto, Product>();

		// ── INVOICE MAPPINGS ──
		CreateMap<Invoice, InvoiceDto>()
			.ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null ? s.Customer.Name : string.Empty))
			.ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatorUser != null ? s.CreatorUser.Name : string.Empty))
			.ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

		// ── INVOICE ITEM MAPPINGS ──
		CreateMap<InvoiceItem, InvoiceItemDto>()
			.ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
			.ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product != null ? s.Product.Code : string.Empty))
			.ForMember(d => d.PackSize, o => o.MapFrom(s => s.Product != null ? s.Product.PackSize : string.Empty));

		// ── USER & AUTH MAPPINGS  ──
		CreateMap<User, UserDto>()
			.ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));

		CreateMap<UpdateProfileDto, User>()
			// Map only Name and Email, ignore all other properties explicitly
			.ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
			.ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
			.ForMember(d => d.PasswordHash, o => o.Ignore())
			.ForMember(d => d.Role, o => o.Ignore())
			.ForMember(d => d.IsApproved, o => o.Ignore())
			.ForMember(d => d.EmailConfirmed, o => o.Ignore())
			.ForMember(d => d.RefreshToken, o => o.Ignore())
			.ForMember(d => d.RefreshTokenExpiry, o => o.Ignore())
			.ForMember(d => d.EmailVerificationToken, o => o.Ignore())
			.ForMember(d => d.CreatedAt, o => o.Ignore())
			.ForMember(d => d.Invoices, o => o.Ignore())
			.ForMember(d => d.Id, o => o.Ignore());

		CreateMap<Register, User>()
			.ForMember(d => d.PasswordHash, o => o.Ignore())
			.ForMember(d => d.Role, o => o.MapFrom(s => s.Role));
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