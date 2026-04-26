using Microsoft.EntityFrameworkCore;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Enums;
using VaatcoBMS.Domain.Interfaces;
using VaatcoBMS.Infrastructure.Persistence;

namespace VaatcoBMS.Infrastructure.Repositories;

public class InvoiceRepository(AppDbContext ctx) : Repository<Invoice>(ctx), IInvoiceRepository
{
	public async Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status)
	{
		return await _set.Where(i => i.Status == status).ToListAsync();
	}

	public async Task<Invoice> GetWithDetailsAsync(int id)
	{
		return await _ctx.Invoices
				.Include(i => i.Customer)
				.Include(i => i.CreatorUser)
				.Include(i => i.Items)
				.Include(i => i.Payments)
				.FirstOrDefaultAsync(i => i.Id == id);
	}
	public async Task<IEnumerable<Invoice>> GetPagedWithDetailsAsync(int page, int pageSize)
	{
		return await _ctx.Invoices
				.Include(i => i.Customer)
				.Include(i => i.CreatorUser)
				.Include(i => i.Items)
				.OrderByDescending(i => i.Id)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();
	}

	public async Task<IEnumerable<Invoice>> GetByCustomerAsync(int customerId)
	{
		return await _set.Where(i => i.CustomerId == customerId).ToListAsync();
	}

	public async Task<IEnumerable<Invoice>> GetByUserAsync(int userId)
	{
		return await _set.Where(i => i.CreatorUser.Id == userId).ToListAsync();
	}

	public async Task<string> GetNextInvoiceNumberAsync()
	{
		var last = await _ctx.Invoices
				.OrderByDescending(i => i.Id)
				.Select(i => i.InvoiceNumber)
				.FirstOrDefaultAsync();

		if (last == null) return "INV-1001";
		var num = int.Parse(last.Split("-")[1]) + 1;
		return $"INV-{num}";
	}
	public async Task<(decimal TotalRevenue, decimal TotalPaid, int TotalCount, int DraftCount, int ApprovedCount, int PaidCount, int CancelledCount)> GetStatsAsync()
	{
		var all = await _set.ToListAsync();
		var totalRevenue = all.Where(i => i.Status != InvoiceStatus.Cancelled).Sum(i => i.TotalAmount);
		var totalPaid = await _ctx.Set<Payment>().SumAsync(p => (decimal?)p.Amount) ?? 0m;
		return (
				totalRevenue,
				totalPaid,
				all.Count,
				all.Count(i => i.Status == InvoiceStatus.Draft),
				all.Count(i => i.Status == InvoiceStatus.Approved),
				all.Count(i => i.Status == InvoiceStatus.Paid),
				all.Count(i => i.Status == InvoiceStatus.Cancelled)
		);
	}
}

