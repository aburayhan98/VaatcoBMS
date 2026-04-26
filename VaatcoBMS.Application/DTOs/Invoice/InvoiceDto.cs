using VaatcoBMS.Application.DTOs.InvoiceItem;
using VaatcoBMS.Application.DTOs.Payment;

namespace VaatcoBMS.Application.DTOs.Invoice;

public class InvoiceDto
{
    public int Id { get; set; } // Add this property to match usage in InvoiceService
    public string InvoiceNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty; // Added this
    public string CreatedByName { get; set; } = string.Empty; // Added this
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal VAT { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<InvoiceItemDto> Items { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
}
