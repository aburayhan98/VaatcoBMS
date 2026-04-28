using VaatcoBMS.Application.DTOs.Product;
using VaatcoBMS.Domain.Common;

namespace VaatcoBMS.Application.Interfaces;

public interface IProductService
{
	Task<IEnumerable<ProductDto>> GetAllAsync();
	Task<IEnumerable<ProductDto>> GetActiveAsync(); 
	Task<ProductDto?> GetByIdAsync(int id);
	Task<ProductDto?> GetByCodeAsync(string code)
		; Task<IEnumerable<ProductDto>> SearchAsync(string keyword);
	Task<IEnumerable<ProductDto>> GetLowStockAsync(int threshold = 20);

	// <summary>Server-side paginated + filtered + sorted list.</summary>
	Task<PagedResult<ProductDto>> GetPagedAsync(ProductQueryParams q);
	Task<ProductDto> CreateAsync(CreateProductDto dto);
	Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto);
	Task AdjustStockAsync(int id, int quantity);  // + add, - deduct 
	Task DeleteAsync(int id); // soft delete
	Task<bool> CodeExistsAsync(string code, int? excludeId = null);
}
