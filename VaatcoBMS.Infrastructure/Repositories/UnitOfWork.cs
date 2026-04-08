using VaatcoBMS.Domain.Interfaces;
using VaatcoBMS.Infrastructure.Persistence;

namespace VaatcoBMS.Infrastructure.Repositories;

public class UnitOfWork(AppDbContext ctx,
	IInvoiceRepository invoices,
	ICustomerRepository customers,
	IProductRepository products,
	IUserRepository users,
	IInvoiceItemRepository invoiceItems) : IUnitOfWork
{
	private readonly AppDbContext _ctx = ctx;
	public IInvoiceRepository Invoices { get; } = invoices;
	public ICustomerRepository Customers { get; } = customers;
	public IProductRepository Products { get; } = products;
	public IUserRepository Users { get; } = users;

	public IInvoiceItemRepository InvoiceItems { get; } = invoiceItems;

	public async Task SaveAsync()
	{
		await _ctx.SaveChangesAsync();
	}
	public async Task<int> SaveChangesAsync()
	{
		return await _ctx.SaveChangesAsync();
	}
	public void Dispose()
	{
		_ctx.Dispose();
	}
}
