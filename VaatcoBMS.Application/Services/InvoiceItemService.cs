using AutoMapper;
using Microsoft.Extensions.Logging;
using VaatcoBMS.Application.DTOs.InvoiceItem;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Enums;
using VaatcoBMS.Domain.Interfaces;

namespace VaatcoBMS.Application.Services;

public class InvoiceItemService(
	IUnitOfWork uow,
	IMapper mapper,
	ILogger<InvoiceItemService> logger) : IInvoiceItemService
{
	private readonly IUnitOfWork _uow = uow;
	private readonly IMapper _mapper = mapper;
	private readonly ILogger<InvoiceItemService> _logger = logger;

	public async Task<InvoiceItemDto> AddItemAsync(int invoiceId, CreateInvoiceItemDto dto)
	{
		try
		{
			// Validate invoice exists and is in Draft status
			var invoice = await _uow.Invoices.GetWithDetailsAsync(invoiceId);
			if (invoice == null)
			{
				_logger.LogWarning("Attempted to add item to non-existent invoice: {InvoiceId}", invoiceId);
				throw new KeyNotFoundException($"Invoice {invoiceId} not found.");
			}

			if (invoice.Status != InvoiceStatus.Draft)
			{
				_logger.LogWarning("Attempted to add item to non-draft invoice {InvoiceId}. Current status: {Status}",
						invoiceId, invoice.Status);
				throw new InvalidOperationException("Items can only be added to Draft invoices.");
			}

			// Validate product exists and has sufficient stock
			var product = await _uow.Products.GetByIdAsync(dto.ProductId);
			if (product == null)
			{
				_logger.LogWarning("Product {ProductId} not found while adding item to invoice {InvoiceId}",
						dto.ProductId, invoiceId);
				throw new KeyNotFoundException($"Product {dto.ProductId} not found.");
			}

			if (product.StockQuantity < dto.Quantity)
			{
				_logger.LogWarning("Insufficient stock for product {ProductId}. Available: {AvailableStock}, Requested: {RequestedQuantity}",
						dto.ProductId, product.StockQuantity, dto.Quantity);
				throw new InvalidOperationException($"Insufficient stock. Available: {product.StockQuantity}.");
			}

			// Create invoice item
			var item = new InvoiceItem
			{
				InvoiceId = invoiceId,
				ProductId = dto.ProductId,
				Quantity = dto.Quantity,
				BonusQuantity = dto.BonusQuantity,
				UnitPrice = dto.UnitPrice > 0 ? dto.UnitPrice : product.Price,
			};

			await _uow.InvoiceItems.AddAsync(item);
			await RecalculateInvoiceTotalsAsync(invoiceId);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("Item added successfully to invoice {InvoiceId}. Product: {ProductId}, Quantity: {Quantity}, UnitPrice: {UnitPrice}",
					invoiceId, dto.ProductId, dto.Quantity, item.UnitPrice);

			return _mapper.Map<InvoiceItemDto>(item);
		}
		catch (KeyNotFoundException)
		{
			throw;
		}
		catch (InvalidOperationException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding item to invoice {InvoiceId} for product {ProductId}",
					invoiceId, dto.ProductId);
			throw new ApplicationException($"An error occurred while adding item to invoice {invoiceId}", ex);
		}
	}

	public async Task<InvoiceItemDto> UpdateItemAsync(int itemId, UpdateInvoiceItemDto dto)
	{
		try
		{
			// Get item or throw not found
			var item = await _uow.InvoiceItems.GetByIdAsync(itemId);
			if (item == null)
			{
				_logger.LogWarning("Invoice item {ItemId} not found for update", itemId);
				throw new KeyNotFoundException($"Invoice item {itemId} not found.");
			}

			// Load parent invoice to check status
			var invoice = await _uow.Invoices.GetByIdAsync(item.InvoiceId);
			if (invoice?.Status != InvoiceStatus.Draft)
			{
				_logger.LogWarning("Attempted to update item {ItemId} on non-draft invoice {InvoiceId}. Status: {Status}",
						itemId, item.InvoiceId, invoice?.Status);
				throw new InvalidOperationException("Only Draft invoice items can be edited.");
			}

			// Store old values for logging
			var oldQuantity = item.Quantity;
			var oldUnitPrice = item.UnitPrice;
			var oldBonusQuantity = item.BonusQuantity;

			// Update fields from DTO
			item.Quantity = dto.Quantity;
			item.BonusQuantity = dto.BonusQuantity;
			item.UnitPrice = dto.UnitPrice;
			item.Total = item.Quantity * item.UnitPrice;

			_uow.InvoiceItems.Update(item);
			await RecalculateInvoiceTotalsAsync(item.InvoiceId);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("Item {ItemId} updated successfully. Quantity: {OldQuantity} → {NewQuantity}, UnitPrice: {OldPrice} → {NewPrice}, BonusQty: {OldBonus} → {NewBonus}",
					itemId, oldQuantity, item.Quantity, oldUnitPrice, item.UnitPrice, oldBonusQuantity, item.BonusQuantity);

			return _mapper.Map<InvoiceItemDto>(item);
		}
		catch (KeyNotFoundException)
		{
			throw;
		}
		catch (InvalidOperationException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating invoice item {ItemId}", itemId);
			throw new ApplicationException($"An error occurred while updating invoice item {itemId}", ex);
		}
	}

	public async Task<InvoiceItemDto> GetByIdAsync(int itemId)
	{
		try
		{
			var item = await _uow.InvoiceItems.GetByIdAsync(itemId);

			if (item == null)
			{
				_logger.LogDebug("Invoice item {ItemId} not found", itemId);
				throw new KeyNotFoundException($"Invoice item {itemId} not found.");
			}

			_logger.LogDebug("Retrieved invoice item {ItemId} for invoice {InvoiceId}", itemId, item.InvoiceId);

			return _mapper.Map<InvoiceItemDto>(item);
		}
		catch (KeyNotFoundException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving invoice item {ItemId}", itemId);
			throw new ApplicationException($"An error occurred while retrieving invoice item {itemId}", ex);
		}
	}

	public async Task<IEnumerable<InvoiceItemDto>> GetByInvoiceAsync(int invoiceId)
	{
		try
		{
			var invoice = await _uow.Invoices.GetWithDetailsAsync(invoiceId);

			if (invoice == null)
			{
				_logger.LogWarning("Invoice {InvoiceId} not found while retrieving items", invoiceId);
				throw new KeyNotFoundException($"Invoice {invoiceId} not found.");
			}

			var itemCount = invoice.Items?.Count() ?? 0;
			_logger.LogDebug("Retrieved {Count} items for invoice {InvoiceId}", itemCount, invoiceId);

			return _mapper.Map<IEnumerable<InvoiceItemDto>>(invoice.Items);
		}
		catch (KeyNotFoundException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving items for invoice {InvoiceId}", invoiceId);
			throw new ApplicationException($"An error occurred while retrieving items for invoice {invoiceId}", ex);
		}
	}

	public async Task RemoveItemAsync(int itemId)
	{
		try
		{
			var item = await _uow.InvoiceItems.GetByIdAsync(itemId);
			if (item == null)
			{
				_logger.LogWarning("Invoice item {ItemId} not found for removal", itemId);
				throw new KeyNotFoundException($"Invoice item {itemId} not found.");
			}

			// Check if invoice is in Draft status
			var invoice = await _uow.Invoices.GetByIdAsync(item.InvoiceId);
			if (invoice?.Status != InvoiceStatus.Draft)
			{
				_logger.LogWarning("Attempted to remove item {ItemId} from non-draft invoice {InvoiceId}. Status: {Status}",
						itemId, item.InvoiceId, invoice?.Status);
				throw new InvalidOperationException("Items can only be removed from Draft invoices.");
			}

			_uow.InvoiceItems.Delete(item);
			await RecalculateInvoiceTotalsAsync(item.InvoiceId);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("Item {ItemId} removed successfully from invoice {InvoiceId}", itemId, item.InvoiceId);
		}
		catch (KeyNotFoundException)
		{
			throw;
		}
		catch (InvalidOperationException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error removing invoice item {ItemId}", itemId);
			throw new ApplicationException($"An error occurred while removing invoice item {itemId}", ex);
		}
	}

	public async Task RecalculateInvoiceTotalsAsync(int invoiceId)
	{
		try
		{
			var invoice = await _uow.Invoices.GetWithDetailsAsync(invoiceId);
			if (invoice == null)
			{
				_logger.LogWarning("Invoice {InvoiceId} not found for recalculation", invoiceId);
				throw new KeyNotFoundException($"Invoice {invoiceId} not found.");
			}

			var oldSubtotal = invoice.Subtotal;
			var oldVAT = invoice.VAT;
			var oldTotal = invoice.TotalAmount;

			// Recalculate totals
			invoice.Subtotal = invoice.Items?.Sum(i => i.Total) ?? 0;
			invoice.VAT = (invoice.Subtotal - invoice.Discount) * (invoice.VAT / 100m);
			invoice.TotalAmount = invoice.Subtotal - invoice.Discount + invoice.VAT;

			_uow.Invoices.Update(invoice);

			_logger.LogDebug("Recalculated totals for invoice {InvoiceId}. Subtotal: {OldSubtotal} → {NewSubtotal}, VAT: {OldVAT} → {NewVAT}, Total: {OldTotal} → {NewTotal}",
					invoiceId, oldSubtotal, invoice.Subtotal, oldVAT, invoice.VAT, oldTotal, invoice.TotalAmount);
		}
		catch (KeyNotFoundException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error recalculating totals for invoice {InvoiceId}", invoiceId);
			throw new ApplicationException($"An error occurred while recalculating totals for invoice {invoiceId}", ex);
		}
	}
}