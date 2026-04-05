using Microsoft.EntityFrameworkCore;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Interfaces;
using VaatcoBMS.Infrastructure.Persistence;

namespace VaatcoBMS.Infrastructure.Repositories;

public class ProductRepository(AppDbContext ctx) : Repository<Product>(ctx), IProductRepository
{
	public async Task<Product> GetByCodeAsync(string code)
	{
		return await _set.FirstOrDefaultAsync(p => p.Code == code);
	}

	public async Task<Product> GetByNameAsync(string name)
	{
		return await _set.FirstOrDefaultAsync(p => p.Name == name);
	}
}
