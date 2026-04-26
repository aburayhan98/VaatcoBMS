using MapsterMapper;
using Microsoft.Extensions.Logging;
using VaatcoBMS.Application.DTOs.Invoice;
using VaatcoBMS.Application.DTOs.Payment;
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

	public async Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto, int userId)
	{
		try
		{
			var customer = await _uow.Customers.GetByIdAsync(dto.CustomerId);
			if (customer == null)
				throw new KeyNotFoundException($"Customer {dto.CustomerId} not found.");

			var invoice = new Invoice
			{
				CustomerId = dto.CustomerId,
				CreatedBy = userId,
				InvoiceDate = dto.IssueDate,
				DueDate = dto.DueDate,
				ReferenceNumber = dto.ReferenceNumber,
				Notes = dto.Notes,
				Discount = dto.Discount ?? 0,
				TaxRate = dto.TaxRate ?? 0,
				Status = InvoiceStatus.Draft,
				CreatedAt = DateTime.UtcNow,
				InvoiceNumber = await _uow.Invoices.GetNextInvoiceNumberAsync(),
			};

			foreach (var itemDto in dto.Items)
			{
				var product = await _uow.Products.GetByIdAsync(itemDto.ProductId);
				if (product == null)
					throw new KeyNotFoundException($"Product {itemDto.ProductId} not found.");

				var unitPrice = itemDto.UnitPrice > 0 ? itemDto.UnitPrice : product.Price;
				invoice.Items.Add(new InvoiceItem
				{
					ProductId = itemDto.ProductId,
					ProductName = product.Name,
					ProductCode = product.Code,
					PackSize = product.PackSize,
					Quantity = itemDto.Quantity,
					BonusQuantity = itemDto.BonusQuantity,
					UnitPrice = unitPrice,
					Total = itemDto.Quantity * unitPrice,
				});
			}

			invoice.Subtotal = invoice.Items.Sum(i => i.Total);
			invoice.VAT = (invoice.Subtotal - invoice.Discount) * (invoice.TaxRate / 100m);
			invoice.TotalAmount = invoice.Subtotal - invoice.Discount + invoice.VAT;

			await _uow.Invoices.AddAsync(invoice);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("Invoice {Number} created by user {UserId}", invoice.InvoiceNumber, userId);
			return await EnrichDtoAsync(_mapper.Map<InvoiceDto>(invoice));
		}
		catch (KeyNotFoundException) { throw; }
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating invoice");
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
				throw new KeyNotFoundException($"Invoice {id} not found.");
			}
			var dto = _mapper.Map<InvoiceDto>(invoice);
			return await EnrichDtoAsync(dto);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving invoice {Id}", id);
			throw new ApplicationException($"Error retrieving invoice {id}.", ex);
		}
	}



	public async Task<IEnumerable<InvoiceDto>> GetAllAsync(int page, int pageSize)
	{
		try
		{
			var invoices = await _uow.Invoices.GetPagedWithDetailsAsync(page, pageSize);
			var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(invoices).ToList();
			foreach (var dto in dtos)
			{
				var paid = await _uow.Payments.GetTotalPaidAsync(dto.Id);
				dto.AmountPaid = paid;
				dto.OutstandingBalance = dto.TotalAmount - paid;
			}
			return dtos;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving invoices page {Page}", page);
			throw new ApplicationException("Error retrieving invoices.", ex);
		}
	}
	public async Task UpdateStatusAsync(int id, InvoiceStatus status)
	{
		try
		{
			var invoice = await _uow.Invoices.GetWithDetailsAsync(id);
			if (invoice == null)
				throw new KeyNotFoundException($"Invoice {id} not found.");

			var oldStatus = invoice.Status;

			// Deduct stock when approving
			if (status == InvoiceStatus.Approved && oldStatus == InvoiceStatus.Draft)
			{
				foreach (var item in invoice.Items)
				{
					var product = await _uow.Products.GetByIdAsync(item.ProductId);
					if (product != null)
					{
						if (product.StockQuantity < item.Quantity)
							throw new InvalidOperationException(
									$"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}, Required: {item.Quantity}.");
						product.StockQuantity -= item.Quantity;
						_uow.Products.Update(product);
					}
				}
			}
			// Restore stock when cancelling an approved invoice
			else if (status == InvoiceStatus.Cancelled && oldStatus == InvoiceStatus.Approved)
			{
				foreach (var item in invoice.Items)
				{
					var product = await _uow.Products.GetByIdAsync(item.ProductId);
					if (product != null)
					{
						product.StockQuantity += item.Quantity;
						_uow.Products.Update(product);
					}
				}
			}

			invoice.Status = status;
			invoice.UpdatedAt = DateTime.UtcNow;
			_uow.Invoices.Update(invoice);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("Invoice {Id} status: {Old} -> {New}", id, oldStatus, status);
		}
		catch (KeyNotFoundException) { throw; }
		catch (InvalidOperationException) { throw; }
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating invoice {Id} status", id);
			throw new ApplicationException($"Error updating invoice status.", ex);
		}
	}
	public async Task<bool> DeleteAsync(int id)
	{
		try
		{
			var invoice = await _uow.Invoices.GetByIdAsync(id);
			if (invoice == null) return false;
			if (invoice.Status != InvoiceStatus.Draft)
				throw new InvalidOperationException("Only draft invoices can be deleted.");
			_uow.Invoices.Delete(invoice);
			await _uow.SaveChangesAsync();
			return true;
		}
		catch (InvalidOperationException) { throw; }
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting invoice {Id}", id);
			throw new ApplicationException($"Error deleting invoice {id}.", ex);
		}
	}
	public async Task<InvoiceStatsDto> GetStatsAsync()
	{
		var (totalRevenue, totalPaid, total, draft, approved, paid, cancelled) =
				await _uow.Invoices.GetStatsAsync();
		return new InvoiceStatsDto
		{
			TotalInvoices = total,
			DraftCount = draft,
			ApprovedCount = approved,
			PaidCount = paid,
			CancelledCount = cancelled,
			TotalRevenue = totalRevenue,
			TotalPaid = totalPaid,
			TotalOutstanding = totalRevenue - totalPaid,
		};
	}

	private async Task<InvoiceDto> EnrichDtoAsync(InvoiceDto dto)
	{
		var payments = await _uow.Payments.GetByInvoiceIdAsync(dto.Id);
		dto.Payments = _mapper.Map<List<PaymentDto>>(payments);
		dto.AmountPaid = dto.Payments.Sum(p => p.Amount);
		dto.OutstandingBalance = dto.TotalAmount - dto.AmountPaid;
		return dto;
	}

	//// Add explicit implementation for IInvoiceService.CreateAsync(InvoiceDto dto, int userId)
	//public async Task<InvoiceDto> CreateAsync(InvoiceDto dto, int userId)
	//{
	//	// Map InvoiceDto to CreateInvoiceDto and call the existing CreateAsync(CreateInvoiceDto, int)
	//	var createDto = _mapper.Map<CreateInvoiceDto>(dto);
	//	return await CreateAsync(createDto, userId);
	//}
}