namespace VaatcoBMS.Application.DTOs.Invoice;

public class UpdateInvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public int CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}
