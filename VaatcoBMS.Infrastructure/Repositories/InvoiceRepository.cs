using Microsoft.EntityFrameworkCore;
using VaatcoBMS.Domain.Common;
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

	public async Task<PagedResult<Invoice>> GetPagedAsync(InvoiceQueryParams q)
	{
		IQueryable<Invoice> query = _ctx.Invoices
			.Include(i => i.Customer)
			.Include(i => i.CreatorUser)
			.AsNoTracking();

		// Filters
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			var term = q.Search.Trim().ToLower();
			query = query.Where(i =>
				i.InvoiceNumber.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
				(i.ReferenceNumber != null && i.ReferenceNumber.Contains(term, StringComparison.CurrentCultureIgnoreCase))
			);
		}

		if (q.CustomerId.HasValue)
			query = query.Where(i => i.CustomerId == q.CustomerId.Value);

		if (!string.IsNullOrWhiteSpace(q.Status) && Enum.TryParse<InvoiceStatus>(q.Status, true, out var statusEnum))
			query = query.Where(i => i.Status == statusEnum);

		if (q.StartDate.HasValue)
			query = query.Where(i => i.InvoiceDate >= q.StartDate.Value);

		if (q.EndDate.HasValue)
			query = query.Where(i => i.InvoiceDate <= q.EndDate.Value);

		var totalCount = await query.CountAsync();

		// Sort
		query = (q.SortBy?.ToLower(), q.SortDir?.ToLower()) switch
		{
			("number", "asc") => query.OrderBy(i => i.InvoiceNumber),
			("number", _) => query.OrderByDescending(i => i.InvoiceNumber),
			("total", "asc") => query.OrderBy(i => i.TotalAmount),
			("total", _) => query.OrderByDescending(i => i.TotalAmount),
			("status", "asc") => query.OrderBy(i => i.Status),
			("status", _) => query.OrderByDescending(i => i.Status),
			("date", "asc") => query.OrderBy(i => i.InvoiceDate),
			_ => query.OrderByDescending(i => i.InvoiceDate) // Default: newest first
		};

		var items = await query
			.Skip((q.Page - 1) * q.PageSize)
			.Take(q.PageSize)
			.ToListAsync();

		return new PagedResult<Invoice>
		{
			Items = items,
			TotalCount = totalCount,
			Page = q.Page,
			PageSize = q.PageSize,
		};
	}
}

