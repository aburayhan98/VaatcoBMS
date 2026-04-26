using VaatcoBMS.Domain.Enums;

namespace VaatcoBMS.Domain.Entities;

public class Invoice : BaseEntityWithAudit
{
	public string InvoiceNumber { get; set; } = string.Empty;
	public int CustomerId { get; set; }

	public DateTime InvoiceDate { get; set; }
	public DateTime? DueDate { get; set; } //new
	public string? ReferenceNumber { get; set; } //new
	public decimal Subtotal { get; set; }
	public decimal Discount { get; set; }
	public decimal TaxRate { get; set; } // percentage e.g. 15 = 15%
	public decimal VAT { get; set; }
	public decimal TotalAmount { get; set; }
	public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft; 
	public string? Notes { get; set; }
	
	public Customer Customer { get; set; } = null!; 
    
	public User CreatorUser { get; set; } = null!; 
	
	public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
	public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}