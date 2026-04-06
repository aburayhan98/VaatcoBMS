

namespace VaatcoBMS.Domain.Entities;

public class InvoiceItem : BaseEntityWithAudit
{
	public int InvoiceId { get; set; }
	public int ProductId { get; set; }
	public int Quantity { get; set; }
	public int BonusQuantity { get; set; } = 0;
	public decimal UnitPrice { get; set; }
	public decimal Total => Quantity * UnitPrice;
	public Invoice Invoice { get; set; } = null!;
	public Product Product { get; set; } = null!;
}
