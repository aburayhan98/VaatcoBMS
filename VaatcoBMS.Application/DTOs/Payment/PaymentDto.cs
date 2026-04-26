namespace VaatcoBMS.Application.DTOs.Payment;

public class PaymentDto
{
	public int Id { get; set; }
	public int InvoiceId { get; set; }
	public decimal Amount { get; set; }
	public DateTime PaymentDate { get; set; }
	public string Method { get; set; } = string.Empty;
	public string? Reference { get; set; }
	public string? Notes { get; set; }
}
