

namespace VaatcoBMS.Domain.Entities;

public class InvoiceItem : BaseEntityWithAudit
{
	public int InvoiceId { get; set; }
	public int ProductId { get; set; }
	public string ProductName { get; set; } = string.Empty; // Denormalized for historical accuracy
	public string ProductCode { get; set; } = string.Empty; // Denormalized for historical accuracy
	public string PackSize { get; set; } = string.Empty; // Denormalized for historical accuracy
	public int Quantity { get; set; }
	public int BonusQuantity { get; set; } = 0;
	public decimal UnitPrice { get; set; }
	public decimal Total { get; set; }
	public Invoice Invoice { get; set; } = null!;
	public Product Product { get; set; } = null!;
}
