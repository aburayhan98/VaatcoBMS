using VaatcoBMS.Application.DTOs.Payment;

namespace VaatcoBMS.Application.Interfaces;

public interface IPaymentService
{
	Task<IEnumerable<PaymentDto>> GetByInvoiceAsync(int invoiceId);
	Task<PaymentDto> CreateAsync(CreatePaymentDto dto);
	Task DeleteAsync(int id);
}
