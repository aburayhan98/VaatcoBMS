using Microsoft.EntityFrameworkCore;
using VaatcoBMS.Domain.Interfaces;
using VaatcoBMS.Infrastructure.Persistence;

namespace VaatcoBMS.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
	protected readonly AppDbContext _ctx;
	protected readonly DbSet<T> _set;
	public Repository(AppDbContext ctx)
	{
		_ctx = ctx;
		_set = _ctx.Set<T>();
	}
	public async Task AddAsync(T entity)
	{
		await _set.AddAsync(entity);
	}

	public void Delete(T entity)
	{
		_set.Remove(entity);
	}

	public async Task<IEnumerable<T>> GetAllAsync()
	{
		return await _set.ToListAsync();
	}

	public async Task<T?> GetByIdAsync(object id)
	{
		return await _set.FindAsync(id);
	}

	public async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize)
	{
		return await _set
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();
	}

	public void Update(T entity)
	{
		_set.Update(entity);
	}
}
