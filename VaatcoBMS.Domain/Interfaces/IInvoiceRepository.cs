

using System.ComponentModel;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Enums;

namespace VaatcoBMS.Domain.Interfaces;

public interface IInvoiceRepository : IRepository<Invoice>
{
	Task<Invoice> GetWithDetailsAsync(int id);
	Task<IEnumerable<Invoice>> GetByCustomerAsync(int customerId);
	Task<IEnumerable<Invoice>> GetByUserAsync(Guid userId); 
	Task<string> GetNextInvoiceNumberAsync(); 
	Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status);
}
