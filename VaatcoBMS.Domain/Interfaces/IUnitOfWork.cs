
namespace VaatcoBMS.Domain.Interfaces;

public interface IUnitOfWork
{
	public interface IUnitOfWork : IDisposable
	{ 
		IInvoiceRepository Invoices { get; }
		ICustomerRepository Customers { get; } 
		IProductRepository Products { get; } 
		IUserRepository Users { get; } Task<int> SaveChangesAsync(); 
	}
}
