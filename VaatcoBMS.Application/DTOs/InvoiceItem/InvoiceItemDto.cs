namespace VaatcoBMS.Application.DTOs.InvoiceItem;

public class InvoiceItemDto
{
	public int ProductId { get; set; }
	public int Quantity { get; set; }
	public int BonusQuantity { get; set; }
	public decimal UnitPrice { get; set; }
}
