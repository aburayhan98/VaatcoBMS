namespace VaatcoBMS.Application.DTOs.Invoice;

public class InvoiceStatsDto
{
	public int TotalInvoices { get; set; }
	public int DraftCount { get; set; }
	public int ApprovedCount { get; set; }
	public int PaidCount { get; set; }
	public int CancelledCount { get; set; }
	public decimal TotalRevenue { get; set; }
	public decimal TotalPaid { get; set; }
	public decimal TotalOutstanding { get; set; }
}
