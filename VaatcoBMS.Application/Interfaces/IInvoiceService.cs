
using VaatcoBMS.Application.DTOs;
using VaatcoBMS.Domain.Enums;

namespace VaatcoBMS.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto> CreateAsync(InvoiceDto dto, int userId);
    Task<InvoiceDto> GetByIdAsync(int id);
    Task<IEnumerable<InvoiceDto>> GetAllAsync(int page, int pageSize);
    Task UpdateStatusAsync(int id, InvoiceStatus status);
    Task<bool> DeleteAsync(int id);
}
