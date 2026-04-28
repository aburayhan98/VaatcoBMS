using VaatcoBMS.Application.DTOs.Invoice;
using VaatcoBMS.Domain.Enums;
using VaatcoBMS.Domain.Common;

namespace VaatcoBMS.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto, int userId);
    Task<InvoiceDto> GetByIdAsync(int id);
    Task<IEnumerable<InvoiceDto>> GetAllAsync(int page, int pageSize);
    Task UpdateStatusAsync(int id, InvoiceStatus status);
	Task<PagedResult<InvoiceDto>> GetPagedAsync(InvoiceQueryParams q);
	Task<InvoiceStatsDto> GetStatsAsync();
	Task<bool> DeleteAsync(int id);
}
