namespace VaatcoBMS.Application.DTOs.InvoiceItem;

public class UpdateInvoiceItemDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
	public int BonusQuantity { get; set; }
	public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
}