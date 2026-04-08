namespace VaatcoBMS.Application.DTOs.InvoiceItem;

public class InvoiceItemDto
{
	public int ProductId { get; set; }
	public string ProductName { get; set; } = string.Empty;
	public string ProductCode { get; set; } = string.Empty;
	public string PackSize { get; set; } = string.Empty;
	public int Quantity { get; set; }
	public int BonusQuantity { get; set; }
	public decimal UnitPrice { get; set; }
}
