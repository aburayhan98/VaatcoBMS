
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Domain.Interfaces;

public interface IInvoiceItemRepository : IRepository<InvoiceItem>
{
	Task<IEnumerable<InvoiceItem>> GetByInvoiceIdAsync(int invoiceId);
	Task<InvoiceItem> GetByIdAsync(int id);
	Task DeleteByIdAsync(int id);
}
