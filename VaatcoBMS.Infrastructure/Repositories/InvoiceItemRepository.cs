using Microsoft.EntityFrameworkCore;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Interfaces;
using VaatcoBMS.Infrastructure.Persistence;

namespace VaatcoBMS.Infrastructure.Repositories;

public class InvoiceItemRepository(AppDbContext context) : Repository<InvoiceItem>(context), IInvoiceItemRepository
{
	public async Task<IEnumerable<InvoiceItem>> GetByInvoiceIdAsync(int invoiceId)
    {
        return await _set.Where(x => x.InvoiceId == invoiceId).ToListAsync();
    }

    public async Task DeleteByIdAsync(int id)
    {
        var item = await GetByIdAsync(id);
        if (item != null)
        {
            Delete(item);
        }
    }

	public async Task<InvoiceItem> GetByIdAsync(int id)
	{
    return await _set.FindAsync(id);
	}
}
