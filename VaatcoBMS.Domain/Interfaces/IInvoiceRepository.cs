using System.ComponentModel;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Enums;

namespace VaatcoBMS.Domain.Interfaces;

public interface IInvoiceRepository : IRepository<Invoice>
{
	Task<Invoice> GetWithDetailsAsync(int id);
	Task<IEnumerable<Invoice>> GetPagedWithDetailsAsync(int page, int pageSize);
	Task<IEnumerable<Invoice>> GetByCustomerAsync(int customerId);
	Task<IEnumerable<Invoice>> GetByUserAsync(int userId); 
	Task<string> GetNextInvoiceNumberAsync(); 
	Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status);
	Task<(decimal TotalRevenue, decimal TotalPaid, int TotalCount, int DraftCount, int ApprovedCount, int PaidCount, int CancelledCount)> GetStatsAsync();
}
