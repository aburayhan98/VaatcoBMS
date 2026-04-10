
using MapsterMapper;
using Microsoft.Extensions.Logging;
using VaatcoBMS.Application.DTOs.Product;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Interfaces;

namespace VaatcoBMS.Application.Services;

public class ProductService(
	IUnitOfWork uow,
	IMapper mapper,
	ILogger<ProductService> logger) : IProductService
{
	private readonly IUnitOfWork _uow = uow;
	private readonly IMapper _mapper = mapper;
	private readonly ILogger<ProductService> _logger = logger;

	public async Task<ProductDto> CreateAsync(CreateProductDto dto)
	{
		try
		{
			// Validate unique product code
			if (await CodeExistsAsync(dto.ProductCode))
			{
				_logger.LogWarning("Attempted to create product with duplicate code: {ProductCode}", dto.ProductCode);
				throw new InvalidOperationException($"Product code '{dto.ProductCode}' already exists.");
			}

			var product = _mapper.Map<Product>(dto);
			product.CreatedAt = DateTime.UtcNow;
			product.IsActive = true;

			await _uow.Products.AddAsync(product);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("Product created successfully. Id: {ProductId}, Code: {ProductCode}, Name: {ProductName}",
					product.Id, product.Code, product.Name);

			return _mapper.Map<ProductDto>(product);
		}
		catch (InvalidOperationException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating product with code: {ProductCode}", dto.ProductCode);
			throw new ApplicationException($"An error occurred while creating the product: {dto.ProductCode}", ex);
		}
	}

	public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto)
	{
		try
		{
			var product = await _uow.Products.GetByIdAsync(id);
			if (product == null)
			{
				_logger.LogWarning("Product with Id: {ProductId} not found for update", id);
				throw new KeyNotFoundException($"Product {id} not found.");
			}

			// Validate unique product code (excluding current product)
			if (await CodeExistsAsync(dto.ProductCode, id))
			{
				_logger.LogWarning("Attempted to update product {ProductId} with duplicate code: {ProductCode}", id, dto.ProductCode);
				throw new InvalidOperationException("Another product already uses this code.");
			}

			_mapper.Map(dto, product);
			_uow.Products.Update(product);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("Product updated successfully. Id: {ProductId}, Code: {ProductCode}", id, product.Code);

			return _mapper.Map<ProductDto>(product);
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
			_logger.LogError(ex, "Error updating product with Id: {ProductId}", id);
			throw new ApplicationException($"An error occurred while updating product with Id: {id}", ex);
		}
	}

	public async Task DeleteAsync(int id)
	{
		try
		{
			var product = await _uow.Products.GetByIdAsync(id);
			if (product == null)
			{
				_logger.LogWarning("Product with Id: {ProductId} not found for deletion", id);
				throw new KeyNotFoundException($"Product {id} not found.");
			}

			// Soft delete
			product.IsActive = false;
			_uow.Products.Update(product);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("Product soft deleted successfully. Id: {ProductId}, Code: {ProductCode}, Name: {ProductName}",
					id, product.Code, product.Name);
		}
		catch (KeyNotFoundException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting product with Id: {ProductId}", id);
			throw new ApplicationException($"An error occurred while deleting product with Id: {id}", ex);
		}
	}

	public async Task AdjustStockAsync(int id, int quantity)
	{
		try
		{
			var product = await _uow.Products.GetByIdAsync(id);
			if (product == null)
			{
				_logger.LogWarning("Product with Id: {ProductId} not found for stock adjustment", id);
				throw new KeyNotFoundException($"Product {id} not found.");
			}

			var newStock = product.StockQuantity + quantity;

			if (newStock < 0)
			{
				_logger.LogWarning("Insufficient stock for product {ProductId}. Available: {AvailableStock}, Requested: {RequestedQuantity}",
						id, product.StockQuantity, Math.Abs(quantity));
				throw new InvalidOperationException($"Insufficient stock. Available: {product.StockQuantity}, Requested: {Math.Abs(quantity)}.");
			}

			var oldStock = product.StockQuantity;
			product.StockQuantity = newStock;

			_uow.Products.Update(product);
			await _uow.SaveChangesAsync();

			var action = quantity > 0 ? "added to" : "deducted from";
			_logger.LogInformation("Stock {Action} product {ProductId}. Old stock: {OldStock}, New stock: {NewStock}, Change: {Quantity}",
					action, id, oldStock, newStock, quantity);
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
			_logger.LogError(ex, "Error adjusting stock for product Id: {ProductId}, Quantity: {Quantity}", id, quantity);
			throw new ApplicationException($"An error occurred while adjusting stock for product Id: {id}", ex);
		}
	}

	public async Task<ProductDto?> GetByIdAsync(int id)
	{
		try
		{
			var product = await _uow.Products.GetByIdAsync(id);

			if (product == null)
			{
				_logger.LogDebug("Product with Id: {ProductId} not found", id);
				return null;
			}

			return _mapper.Map<ProductDto>(product);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving product by Id: {ProductId}", id);
			throw new ApplicationException($"An error occurred while retrieving product with Id: {id}", ex);
		}
	}

	public async Task<ProductDto?> GetByCodeAsync(string code)
	{
		try
		{
			var product = await _uow.Products.GetByCodeAsync(code);

			if (product == null)
			{
				_logger.LogDebug("Product with code: {ProductCode} not found", code);
				return null;
			}

			return _mapper.Map<ProductDto>(product);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving product by code: {ProductCode}", code);
			throw new ApplicationException($"An error occurred while retrieving product with code: {code}", ex);
		}
	}

	public async Task<IEnumerable<ProductDto>> GetAllAsync()
	{
		try
		{
			var products = await _uow.Products.GetAllAsync();
			var productCount = products.Count();

			_logger.LogDebug("Retrieved {Count} products from database", productCount);

			return _mapper.Map<IEnumerable<ProductDto>>(products);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving all products");
			throw new ApplicationException("An error occurred while retrieving all products", ex);
		}
	}

	public async Task<IEnumerable<ProductDto>> GetActiveAsync()
	{
		try
		{
			var products = await _uow.Products.GetAllAsync();
			var activeProducts = products.Where(p => p.IsActive);
			var activeCount = activeProducts.Count();

			_logger.LogDebug("Retrieved {Count} active products", activeCount);

			return _mapper.Map<IEnumerable<ProductDto>>(activeProducts);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving active products");
			throw new ApplicationException("An error occurred while retrieving active products", ex);
		}
	}

	public async Task<IEnumerable<ProductDto>> GetLowStockAsync(int threshold = 20)
	{
		try
		{
			var all = await _uow.Products.GetAllAsync();
			var lowStockProducts = all.Where(p => p.IsActive && p.StockQuantity <= threshold);
			var lowStockCount = lowStockProducts.Count();

			_logger.LogInformation("Found {Count} products with low stock (threshold: {Threshold})", lowStockCount, threshold);

			return _mapper.Map<IEnumerable<ProductDto>>(lowStockProducts);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving low stock products with threshold: {Threshold}", threshold);
			throw new ApplicationException($"An error occurred while retrieving low stock products", ex);
		}
	}

	public async Task<IEnumerable<ProductDto>> SearchAsync(string keyword)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(keyword))
			{
				_logger.LogDebug("Search called with empty keyword, returning active products");
				return await GetActiveAsync();
			}

			var all = await _uow.Products.GetAllAsync();

			var result = all.Where(p =>
					p.IsActive &&
					(p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
					 p.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
					 p.PackSize.Contains(keyword, StringComparison.OrdinalIgnoreCase))
			);

			var resultCount = result.Count();
			_logger.LogDebug("Search for '{Keyword}' returned {Count} results", keyword, resultCount);

			return _mapper.Map<IEnumerable<ProductDto>>(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error searching products with keyword: {Keyword}", keyword);
			throw new ApplicationException($"An error occurred while searching products with keyword: {keyword}", ex);
		}
	}

	public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
	{
		try
		{
			var all = await _uow.Products.GetAllAsync();

			var exists = all.Any(p =>
					p.Code.Equals(code, StringComparison.OrdinalIgnoreCase) &&
					(excludeId == null || p.Id != excludeId)
			);

			if (exists)
			{
				_logger.LogDebug("Product code '{Code}' already exists (ExcludeId: {ExcludeId})", code, excludeId);
			}

			return exists;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error checking if product code exists: {Code}", code);
			throw new ApplicationException($"An error occurred while checking product code: {code}", ex);
		}
	}
}