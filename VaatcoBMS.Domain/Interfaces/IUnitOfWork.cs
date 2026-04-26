namespace VaatcoBMS.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{ 
    IInvoiceRepository Invoices { get; }
    ICustomerRepository Customers { get; } 
    IProductRepository Products { get; } 
    IUserRepository Users { get; } 
    IInvoiceItemRepository InvoiceItems { get; }
	 IPaymentRepository Payments { get; }
	Task<int> SaveChangesAsync();
}
