using MapsterMapper;
using Microsoft.Extensions.Logging;
using VaatcoBMS.Application.DTOs.Payment;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Enums;
using VaatcoBMS.Domain.Interfaces;

namespace VaatcoBMS.Application.Services;

public class PaymentService(
		IUnitOfWork uow,
		IMapper mapper,
		ILogger<PaymentService> logger) : IPaymentService
{
	private readonly IUnitOfWork _uow = uow;
	private readonly IMapper _mapper = mapper;
	private readonly ILogger<PaymentService> _logger = logger;

	public async Task<IEnumerable<PaymentDto>> GetByInvoiceAsync(int invoiceId)
	{
		var payments = await _uow.Payments.GetByInvoiceIdAsync(invoiceId);
		return _mapper.Map<IEnumerable<PaymentDto>>(payments);
	}

	public async Task<PaymentDto> CreateAsync(CreatePaymentDto dto)
	{
		var invoice = await _uow.Invoices.GetByIdAsync(dto.InvoiceId);
		if (invoice == null)
			throw new KeyNotFoundException($"Invoice {dto.InvoiceId} not found.");
		if (invoice.Status == InvoiceStatus.Cancelled)
			throw new InvalidOperationException("Cannot record payment against a cancelled invoice.");
		if (invoice.Status == InvoiceStatus.Draft)
			throw new InvalidOperationException("Invoice must be approved before recording payments.");

		var totalPaid = await _uow.Payments.GetTotalPaidAsync(dto.InvoiceId);
		var outstanding = invoice.TotalAmount - totalPaid;

		if (dto.Amount <= 0)
			throw new InvalidOperationException("Payment amount must be greater than zero.");
		if (dto.Amount > outstanding)
			throw new InvalidOperationException(
					$"Payment ৳{dto.Amount:N2} exceeds outstanding balance ৳{outstanding:N2}.");

		var payment = new Payment
		{
			InvoiceId = dto.InvoiceId,
			Amount = dto.Amount,
			PaymentDate = dto.PaymentDate,
			Method = dto.Method,
			Reference = dto.Reference,
			Notes = dto.Notes,
			CreatedAt = DateTime.UtcNow,
		};

		await _uow.Payments.AddAsync(payment);

		if (totalPaid + dto.Amount >= invoice.TotalAmount)
		{
			invoice.Status = InvoiceStatus.Paid;
			_uow.Invoices.Update(invoice);
		}

		await _uow.SaveChangesAsync();
		_logger.LogInformation("Payment ৳{Amount} recorded for invoice {Id}", dto.Amount, dto.InvoiceId);
		return _mapper.Map<PaymentDto>(payment);
	}

	public async Task DeleteAsync(int id)
	{
		var payment = await _uow.Payments.GetByIdAsync(id);
		if (payment == null)
			throw new KeyNotFoundException($"Payment {id} not found.");

		var invoice = await _uow.Invoices.GetByIdAsync(payment.InvoiceId);
		if (invoice?.Status == InvoiceStatus.Paid)
		{
			invoice.Status = InvoiceStatus.Approved;
			_uow.Invoices.Update(invoice);
		}

		_uow.Payments.Delete(payment);
		await _uow.SaveChangesAsync();
	}
}
