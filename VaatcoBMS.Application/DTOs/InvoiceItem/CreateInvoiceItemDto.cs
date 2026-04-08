namespace VaatcoBMS.Application.DTOs.InvoiceItem;

public class CreateInvoiceItemDto
{
	public int InvoiceId { get; set; }
	public int ProductId { get; set; }
	public string ProductName { get; set; } = string.Empty;
	public string ProductCode { get; set; } = string.Empty;
	public string PackSize { get; set; } = string.Empty;
	public int Quantity { get; set; }
	public int BonusQuantity { get; set; } = 0;
	public decimal UnitPrice { get; set; } // 0 = use product default price
}

