using AutoMapper;
using VaatcoBMS.Application.DTOs;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Enums;
using VaatcoBMS.Domain.Interfaces;

namespace VaatcoBMS.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public InvoiceService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<InvoiceDto> CreateAsync(InvoiceDto dto, int userId)
    {
		// 1. Map to Entity
		var invoice = new Invoice
		{
			  CustomerId = dto.CustomerId,
				CreatedBy = userId, // Updated from CreatedByUserId
        InvoiceDate = dto.InvoiceDate,
				Notes = dto.Notes,
				Discount = dto.Discount,
				Status = InvoiceStatus.Draft,
        CreatedAt = DateTime.UtcNow,
			// 2. Generate Number
			InvoiceNumber = await _uow.Invoices.GetNextInvoiceNumberAsync(),

			// 3. Map Items 
			Items = [.. dto.Items.Select(i => new InvoiceItem
			{
				ProductId = i.ProductId,
				Quantity = i.Quantity,
				BonusQuantity = i.BonusQuantity,
				UnitPrice = i.UnitPrice
				// Total is a computed property in the entity (Quantity * UnitPrice), so we don't set it here
			})]
		};

		// 4. Calculate Financials
		invoice.Subtotal = invoice.Items.Sum(i => i.Total);
        invoice.VAT = (invoice.Subtotal - invoice.Discount) * (dto.VAT / 100m);
        invoice.TotalAmount = invoice.Subtotal - invoice.Discount + invoice.VAT;

        // 5. Save
        await _uow.Invoices.AddAsync(invoice);
        await _uow.SaveChangesAsync(); // Depends on how you named it in IUnitOfWork (SaveAsync or SaveChangesAsync)

        // 6. Return mapped result
        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<InvoiceDto> GetByIdAsync(int id)
    {
        var invoice = await _uow.Invoices.GetWithDetailsAsync(id);
        
        if (invoice == null) return null;

        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<IEnumerable<InvoiceDto>> GetAllAsync(int page, int pageSize)
    {
        var invoices = await _uow.Invoices.GetPagedAsync(page, pageSize);
        return _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
    }

    public async Task UpdateStatusAsync(int id, InvoiceStatus status)
    {
        var invoice = await _uow.Invoices.GetByIdAsync(id);
        
        if (invoice != null)
        {
            invoice.Status = status;
            invoice.UpdatedAt = DateTime.UtcNow;
            
            _uow.Invoices.Update(invoice);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var invoice = await _uow.Invoices.GetByIdAsync(id);
        
        if (invoice == null) return false;

        // Business Rule: Can only delete Draft invoices
        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new InvalidOperationException("Only draft invoices can be deleted.");
        }

        _uow.Invoices.Delete(invoice);
        await _uow.SaveChangesAsync();

        return true;
    }
}
