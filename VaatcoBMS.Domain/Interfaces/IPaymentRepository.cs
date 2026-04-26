using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Domain.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
	Task<IEnumerable<Payment>> GetByInvoiceIdAsync(int invoiceId);
	Task<decimal> GetTotalPaidAsync(int invoiceId);
}
