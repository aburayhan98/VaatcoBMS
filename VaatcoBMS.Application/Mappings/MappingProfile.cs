using Mapster;
using VaatcoBMS.Application.DTOs.Customer;
using VaatcoBMS.Application.DTOs.Invoice;
using VaatcoBMS.Application.DTOs.InvoiceItem;
using VaatcoBMS.Application.DTOs.Payment;
using VaatcoBMS.Application.DTOs.Product;
using VaatcoBMS.Application.DTOs.User;
using VaatcoBMS.Application.Model.Auth;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Enums;

namespace VaatcoBMS.Application.Mappings;

public class MappingProfile : IRegister
{
	public void Register(TypeAdapterConfig config)
	{
		// ── CUSTOMER MAPPINGS ──
		config.NewConfig<Customer, CustomerDto>()
			.Map(dest => dest.TotalInvoices, src => src.Invoices != null ? src.Invoices.Count : 0);

		// ── PRODUCT MAPPINGS ──
		config.NewConfig<Product, ProductDto>()
			.Map(dest => dest.ProductCode, src => src.Code)
			.Map(dest => dest.ProductName, src => src.Name)
			.Map(dest => dest.StockStatus, src => MapStockStatus(src.StockStatus));

		config.NewConfig<CreateProductDto, Product>()
			.Map(dest => dest.Code, src => src.ProductCode)
			.Map(dest => dest.Name, src => src.ProductName);

		config.NewConfig<UpdateProductDto, Product>()
			.Ignore(dest => dest.Id)
			.Map(dest => dest.Code, src => src.ProductCode)
			.Map(dest => dest.Name, src => src.ProductName)
			.Map(dest => dest.PackSize, src => src.PackSize)
			.Map(dest => dest.Price, src => src.Price)
			.Map(dest => dest.Code, src => src.ProductCode);
		// ── INVOICE MAPPINGS ──
		config.NewConfig<Invoice, InvoiceDto>()
			.Map(dest => dest.CustomerName, src => src.Customer != null ? src.Customer.Name : string.Empty)
			.Map(dest => dest.CreatedByName, src => src.CreatorUser != null ? src.CreatorUser.Name : string.Empty)
			.Map(dest => dest.Status, src => src.Status.ToString());

		// ── INVOICE ITEM MAPPINGS ──
		config.NewConfig<InvoiceItem, InvoiceItemDto>()
			.Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : string.Empty)
			.Map(dest => dest.ProductCode, src => src.Product != null ? src.Product.Code : string.Empty)
			.Map(dest => dest.PackSize, src => src.Product != null ? src.Product.PackSize : string.Empty);

		// ── PAYMENT MAPPINGS ──
		config.NewConfig<Payment, PaymentDto>();

		// ── USER & AUTH MAPPINGS  ──
		config.NewConfig<User, UserDto>()
			.Map(dest => dest.Role, src => src.Role.ToString());

		// Mapster naturally ignores missing properties, but we explicitly tell it to only map Name and Email
		config.NewConfig<UpdateProfileDto, User>()
			.Map(dest => dest.Name, src => src.Name)
			.Map(dest => dest.Email, src => src.Email)
			.IgnoreNonMapped(true); 

		config.NewConfig<Register, User>()
			.Map(dest => dest.Role, src => src.Role)
			.Ignore(dest => dest.PasswordHash);
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