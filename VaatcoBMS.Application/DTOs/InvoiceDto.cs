namespace VaatcoBMS.Application.DTOs;

public class CreateInvoice
{
	public int CustomerId { get; set; }
	public DateTime InvoiceDate { get; set; }
	public decimal Discount { get; set; }
	public decimal VATPercent { get; set; }
	public string Notes { get; set; }
	public List<InvoiceItemDto> Items { get; set; } = new();
}
