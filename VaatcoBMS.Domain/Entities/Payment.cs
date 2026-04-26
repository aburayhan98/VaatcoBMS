namespace VaatcoBMS.Domain.Entities;

public class Payment : BaseEntity
{
	public int InvoiceId { get; set; }
	public decimal Amount { get; set; }
	public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
	public string Method { get; set; } = "Cash"; // Cash | BankTransfer | Cheque | MobileBanking
	public string? Reference { get; set; }
	public string? Notes { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public Invoice Invoice { get; set; } = null!;
}
