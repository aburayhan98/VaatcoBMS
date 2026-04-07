using VaatcoBMS.Application.DTOs.InvoiceItem;

namespace VaatcoBMS.Application.DTOs.Invoice;

public class CreateInvoiceDto
{
    public int CustomerId { get; set; }
    
    public DateTime IssueDate { get; set; }
    
    public DateTime DueDate { get; set; }
    
    public string? ReferenceNumber { get; set; }
    
    public string? Notes { get; set; }
    
    public decimal? Discount { get; set; }
    
    public decimal? TaxRate { get; set; }

    public List<CreateInvoiceItemDto> Items { get; set; } = [];
}