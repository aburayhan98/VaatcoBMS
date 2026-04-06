namespace VaatcoBMS.Application.DTOs;

public class InvoiceDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty; // Added this
    public string CreatedByName { get; set; } = string.Empty; // Added this
    public DateTime InvoiceDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal VAT { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<InvoiceItemDto> Items { get; set; } = new();
}
