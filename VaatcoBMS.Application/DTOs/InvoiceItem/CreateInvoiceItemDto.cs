
namespace VaatcoBMS.Application.DTOs.InvoiceItem;

public class CreateInvoiceItemDto
{
	public int ProductId { get; set; }
	public int Quantity { get; set; }
	public int BonusQuantity { get; set; } = 0;
	public decimal UnitPrice { get; set; } // 0 = use product default price
}

