using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
	// Add any product-specific repository methods here, e.g., GetByCodeAsync(string code)
	Task<Product?> GetByCodeAsync(string code);
	Task<Product> GetByNameAsync(string name);
}
