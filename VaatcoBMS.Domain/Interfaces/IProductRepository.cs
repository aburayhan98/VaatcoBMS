using VaatcoBMS.Domain.Common;
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
	// Add any product-specific repository methods here, e.g., GetByCodeAsync(string code)
	Task<Product?> GetByCodeAsync(string code);

	/// <summary>
	/// Server-side filtered, sorted, paged query.
	/// Single round-trip: COUNT + SELECT in one EF execution.
	/// </summary>
	Task<PagedResult<Product>> GetPagedAsync(ProductQueryParams q);
	Task<Product> GetByNameAsync(string name);

	/// <summary>Returns products whose stock is at or below threshold.</summary>
	Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 20);
}
