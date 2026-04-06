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
				.Include(i => i.CreatedBy)
				.Include(i => i.Items)
				// Note: Make sure the Product navigation property is uncommented in InvoiceItem.cs
				// .ThenInclude(item => item.Product) 
				.FirstOrDefaultAsync(i => i.Id == id);
	}

	public async Task<IEnumerable<Invoice>> GetByCustomerAsync(int customerId)
	{
		return await _set.Where(i => i.CustomerId == customerId).ToListAsync();
	}

	public async Task<IEnumerable<Invoice>> GetByUserAsync(int userId)
	{
		return await _set.Where(i => i.CreatedByUserId == userId).ToListAsync();
	}

	public async Task<string> GetNextInvoiceNumberAsync()
	{
		var lastInvoice = await _ctx.Invoices
			.OrderByDescending(i => i.Id).Select(i => i.InvoiceNumber )
			.FirstOrDefaultAsync();

		if (lastInvoice == null)
		{
			return "INV-1001";
		}

		var num = int.Parse(lastInvoice.Split("-")[1]) + 1; 
		return $"INV-{num}";
	}
}

