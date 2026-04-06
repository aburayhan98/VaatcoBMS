using VaatcoBMS.Domain.Interfaces;
using VaatcoBMS.Infrastructure.Persistence;

namespace VaatcoBMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
	private readonly AppDbContext _ctx;
	public IInvoiceRepository Invoices { get; }
	public ICustomerRepository Customers { get; }
	public IProductRepository Products { get; }
	public IUserRepository Users { get; }
	public UnitOfWork(AppDbContext ctx, 
		IInvoiceRepository invoices, 
		ICustomerRepository customers, 
		IProductRepository products,
		IUserRepository users)
	{
		_ctx = ctx;
		Invoices = invoices;
		Customers = customers;
		Products = products;
		Users = users;
	}
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
