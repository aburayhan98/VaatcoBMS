using Microsoft.EntityFrameworkCore;
using VaatcoBMS.Domain.Common;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Enums;
using VaatcoBMS.Domain.Interfaces;
using VaatcoBMS.Infrastructure.Persistence;

namespace VaatcoBMS.Infrastructure.Repositories;

public class ProductRepository(AppDbContext ctx) : Repository<Product>(ctx), IProductRepository
{
	public async Task<Product> GetByCodeAsync(string code)
	{
		return await _set.FirstOrDefaultAsync(p => p.Code == code);
	}
	public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 20)
				=> await _set
						.AsNoTracking()
						.Where(p => p.IsActive && p.StockQuantity <= threshold)
						.OrderBy(p => p.StockQuantity)
						.ToListAsync();

	// ── server-side paging ─────────────────────────────────────────────────────

	public async Task<PagedResult<Product>> GetPagedAsync(ProductQueryParams q)
	{
		// 1. Base query – no tracking, no data yet
		IQueryable<Product> query = _set.AsNoTracking();

		// 2. Filters
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			// EF Core translates this to a parameterised LIKE '%term%'
			var term = q.Search.Trim().ToLower();
			query = query.Where(p =>
					p.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
					p.Code.Contains(term, StringComparison.CurrentCultureIgnoreCase));
		}

		if (q.IsActive.HasValue)
			query = query.Where(p => p.IsActive == q.IsActive.Value);

		if (!string.IsNullOrWhiteSpace(q.StockStatus))
		{
			query = q.StockStatus switch
			{
				"OK" => query.Where(p => p.StockStatus == StockStatus.InStock),
				"Low" => query.Where(p => p.StockStatus == StockStatus.LowStock),
				"Out of Stock" => query.Where(p => p.StockStatus == StockStatus.OutOfStock),
				_ => query
			};
		}

		// 3. Total count – single COUNT(*) before paging
		var totalCount = await query.CountAsync();

		// 4. Sort
		query = (q.SortBy.ToLower(), q.SortDir.ToLower()) switch
		{
			("code", "desc") => query.OrderByDescending(p => p.Code),
			("code", _) => query.OrderBy(p => p.Code),
			("price", "desc") => query.OrderByDescending(p => p.Price),
			("price", _) => query.OrderBy(p => p.Price),
			("stock", "desc") => query.OrderByDescending(p => p.StockQuantity),
			("stock", _) => query.OrderBy(p => p.StockQuantity),
			(_, "desc") => query.OrderByDescending(p => p.Name),
			_ => query.OrderBy(p => p.Name),
		};

		// 5. Page – SQL OFFSET / FETCH NEXT
		var items = await query
				.Skip((q.Page - 1) * q.PageSize)
				.Take(q.PageSize)
				.ToListAsync();

		return new PagedResult<Product>
		{
			Items = items,
			TotalCount = totalCount,
			Page = q.Page,
			PageSize = q.PageSize,
		};
	}
	public async Task<Product> GetByNameAsync(string name)
	{
		return await _set.FirstOrDefaultAsync(p => p.Name == name);
	}
}
