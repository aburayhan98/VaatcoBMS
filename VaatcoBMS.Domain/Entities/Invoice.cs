

namespace VaatcoBMS.Domain.Entities;

public class Invoice
{
	public int Id { get; set; }
	public string InvoiceNumber { get; set; } = string.Empty; public int CustomerId { get; set; }
	public Guid CreatedByUserId { get; set; }
	public DateTime InvoiceDate { get; set; }
	public decimal Subtotal { get; set; }
	public decimal Discount { get; set; }
	public decimal VAT { get; set; }
	public decimal TotalAmount { get; set; }
	public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft; 
	public string? Notes { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
	public DateTime? UpdatedAt { get; set; }
}// Navigation properties public Customer Customer { get; set; } = null!; public User CreatedBy { get; set; } = null!; public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();