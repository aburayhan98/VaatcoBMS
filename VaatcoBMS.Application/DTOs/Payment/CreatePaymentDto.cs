namespace VaatcoBMS.Application.DTOs.Payment;

public class CreatePaymentDto
{
	public int InvoiceId { get; set; }
	public decimal Amount { get; set; }
	public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
	public string Method { get; set; } = "Cash";
	public string? Reference { get; set; }
	public string? Notes { get; set; }
}
