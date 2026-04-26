using Microsoft.EntityFrameworkCore;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Interfaces;
using VaatcoBMS.Infrastructure.Persistence;

namespace VaatcoBMS.Infrastructure.Repositories;

public class PaymentRepository(AppDbContext ctx) : Repository<Payment>(ctx), IPaymentRepository
{
	public async Task<IEnumerable<Payment>> GetByInvoiceIdAsync(int invoiceId)
	{
		return await _set
				.Where(p => p.InvoiceId == invoiceId)
				.OrderByDescending(p => p.PaymentDate)
				.ToListAsync();
	}

	public async Task<decimal> GetTotalPaidAsync(int invoiceId)
	{
		return await _set
				.Where(p => p.InvoiceId == invoiceId)
				.SumAsync(p => (decimal?)p.Amount) ?? 0m;
	}
}
