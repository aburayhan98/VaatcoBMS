using VaatcoBMS.Application.DTOs.InvoiceItem;

namespace VaatcoBMS.Application.Interfaces;

public interface IInvoiceItemService
{
	Task<IEnumerable<InvoiceItemDto>> GetByInvoiceAsync(int invoiceId);
	Task<InvoiceItemDto> GetByIdAsync(int itemId);
	Task<InvoiceItemDto> AddItemAsync(int invoiceId, CreateInvoiceItemDto dto); 
	Task<InvoiceItemDto> UpdateItemAsync(int itemId, UpdateInvoiceItemDto dto); 
	Task RemoveItemAsync(int itemId);
	Task RecalculateInvoiceTotalsAsync(int invoiceId);
}
