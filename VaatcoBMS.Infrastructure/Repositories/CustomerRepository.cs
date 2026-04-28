using Microsoft.EntityFrameworkCore;
using VaatcoBMS.Domain.Common;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Interfaces;
using VaatcoBMS.Infrastructure.Persistence;

namespace VaatcoBMS.Infrastructure.Repositories;

public class CustomerRepository(AppDbContext ctx) : Repository<Customer>(ctx), ICustomerRepository
{
	public async Task<Customer> GetByCustomerAsync(string contactPerson, CancellationToken cancellationToken = default)
	{
		return await _set.FirstOrDefaultAsync(c => c.ContactPerson == contactPerson, cancellationToken);
	}
	public async Task<Customer> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
	{
		return await _set.FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
	}

	public async Task<Customer> GetByNameAsync(string name, CancellationToken cancellationToken = default)
	{
		return await _set.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
	}

	public async Task<PagedResult<Customer>> GetPagedAsync(CustomerQueryParams q)
	{
		// 1. Base query – no tracking, no data yet
		IQueryable<Customer> query = _set.AsNoTracking();

		// 2. Filters
		if (!string.IsNullOrWhiteSpace(q.Search))
		{
			// EF Core translates this to a parameterised LIKE '%term%'
			var term = q.Search.Trim().ToLower();
			query = query.Where(c =>
					c.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
					(c.CustomerCode != null && c.CustomerCode.Contains(term, StringComparison.CurrentCultureIgnoreCase)) ||
					(c.ContactPerson != null && c.ContactPerson.Contains(term, StringComparison.CurrentCultureIgnoreCase)) ||
					c.Phone.Contains(term, StringComparison.CurrentCultureIgnoreCase));
		}

		if (!string.IsNullOrWhiteSpace(q.City))
		{
			query = query.Where(c => c.City != null && c.City.ToLower() == q.City.Trim().ToLower());
		}

		if (!string.IsNullOrWhiteSpace(q.District))
		{
			query = query.Where(c => c.District != null && c.District.ToLower() == q.District.Trim().ToLower());
		}

		// Assuming you add an IsActive bool flag to Customer domain Model in the future
		// if (q.IsActive.HasValue)
		// 	query = query.Where(c => c.IsActive == q.IsActive.Value);

		// 3. Total count – single COUNT(*) before paging
		var totalCount = await query.CountAsync();

		// 4. Sort
		query = (q.SortBy?.ToLower(), q.SortDir?.ToLower()) switch
		{
			("code", "desc") => query.OrderByDescending(c => c.CustomerCode),
			("code", _) => query.OrderBy(c => c.CustomerCode),
			("city", "desc") => query.OrderByDescending(c => c.City),
			("city", _) => query.OrderBy(c => c.City),
			("createdat", "desc") => query.OrderByDescending(c => c.Id), // Id acts as chronological created sort fallback
			("createdat", _) => query.OrderBy(c => c.Id),
			(_, "desc") => query.OrderByDescending(c => c.Name),
			_ => query.OrderBy(c => c.Name), // Default sorting
		};

		// 5. Page – SQL OFFSET / FETCH NEXT
		var items = await query
				.Skip((q.Page - 1) * q.PageSize)
				.Take(q.PageSize)
				.ToListAsync();

		return new PagedResult<Customer>
		{
			Items = items,
			TotalCount = totalCount,
			Page = q.Page,
			PageSize = q.PageSize,
		};
	}

	public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
	{
		// If there's NO customer with this email, then it's unique (returns true)
		return !await _set.AnyAsync(c => c.Email == email, cancellationToken);
	}

	public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(searchTerm))
		{
			return Enumerable.Empty<Customer>();
		}

		return await _set
				.Where(c => c.Name.Contains(searchTerm) ||
										(c.Email != null && c.Email.Contains(searchTerm)) ||
										(c.ContactPerson != null && c.ContactPerson.Contains(searchTerm)) ||
										c.Phone.Contains(searchTerm))
				.ToListAsync(cancellationToken);
	}
}
