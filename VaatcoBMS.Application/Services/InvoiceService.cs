using AutoMapper;
using Microsoft.Extensions.Logging;
using VaatcoBMS.Application.DTOs.Invoice;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Enums;
using VaatcoBMS.Domain.Interfaces;

namespace VaatcoBMS.Application.Services;

public class InvoiceService(
	IUnitOfWork uow,
	IMapper mapper,
	ILogger<InvoiceService> logger) : IInvoiceService
{
	private readonly IUnitOfWork _uow = uow;
	private readonly IMapper _mapper = mapper;
	private readonly ILogger<InvoiceService> _logger = logger;

	public async Task<InvoiceDto> CreateAsync(InvoiceDto dto, int userId)
	{
		try
		{
			// 1. Map to Entity
			var invoice = new Invoice
			{
				CustomerId = dto.CustomerId,
				CreatedBy = userId,
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
			await _uow.SaveChangesAsync();

			_logger.LogInformation("Invoice created successfully. Invoice Number: {InvoiceNumber}, UserId: {UserId}",
					invoice.InvoiceNumber, userId);

			// 6. Return mapped result
			return _mapper.Map<InvoiceDto>(invoice);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while creating invoice for CustomerId: {CustomerId}, UserId: {UserId}",
					dto.CustomerId, userId);
			throw new ApplicationException("An error occurred while creating the invoice.", ex);
		}
	}

	public async Task<InvoiceDto> GetByIdAsync(int id)
	{
		try
		{
			var invoice = await _uow.Invoices.GetWithDetailsAsync(id);

			if (invoice == null)
			{
				_logger.LogWarning("Invoice with Id: {InvoiceId} was not found", id);
				return null;
			}

			return _mapper.Map<InvoiceDto>(invoice);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while retrieving invoice with Id: {InvoiceId}", id);
			throw new ApplicationException($"An error occurred while retrieving invoice with Id: {id}.", ex);
		}
	}

	public async Task<IEnumerable<InvoiceDto>> GetAllAsync(int page, int pageSize)
	{
		try
		{
			var invoices = await _uow.Invoices.GetPagedAsync(page, pageSize);
			_logger.LogInformation("Retrieved {Count} invoices for page {Page} with page size {PageSize}",
					invoices.Count(), page, pageSize);
			return _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while retrieving invoices for page: {Page}, pageSize: {PageSize}",
					page, pageSize);
			throw new ApplicationException("An error occurred while retrieving invoices.", ex);
		}
	}

	public async Task UpdateStatusAsync(int id, InvoiceStatus status)
	{
		try
		{
			var invoice = await _uow.Invoices.GetByIdAsync(id);

			if (invoice == null)
			{
				_logger.LogWarning("Cannot update status. Invoice with Id: {InvoiceId} was not found", id);
				throw new KeyNotFoundException($"Invoice {id} not found.");
			}

			var oldStatus = invoice.Status;
			invoice.Status = status;
			invoice.UpdatedAt = DateTime.UtcNow;

			_uow.Invoices.Update(invoice);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("Invoice status updated successfully. InvoiceId: {InvoiceId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
					id, oldStatus, status);
		}
		catch (KeyNotFoundException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while updating status for invoice Id: {InvoiceId} to status: {Status}",
					id, status);
			throw new ApplicationException($"An error occurred while updating status for invoice Id: {id}.", ex);
		}
	}

	public async Task<bool> DeleteAsync(int id)
	{
		try
		{
			var invoice = await _uow.Invoices.GetByIdAsync(id);

			if (invoice == null)
			{
				_logger.LogWarning("Cannot delete. Invoice with Id: {InvoiceId} was not found", id);
				return false;
			}

			// Business Rule: Can only delete Draft invoices
			if (invoice.Status != InvoiceStatus.Draft)
			{
				_logger.LogWarning("Attempted to delete non-draft invoice. InvoiceId: {InvoiceId}, CurrentStatus: {Status}",
						id, invoice.Status);
				throw new InvalidOperationException("Only draft invoices can be deleted.");
			}

			_uow.Invoices.Delete(invoice);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("Invoice deleted successfully. InvoiceId: {InvoiceId}, InvoiceNumber: {InvoiceNumber}",
					id, invoice.InvoiceNumber);

			return true;
		}
		catch (InvalidOperationException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while deleting invoice with Id: {InvoiceId}", id);
			throw new ApplicationException($"An error occurred while deleting invoice with Id: {id}.", ex);
		}
	}
}