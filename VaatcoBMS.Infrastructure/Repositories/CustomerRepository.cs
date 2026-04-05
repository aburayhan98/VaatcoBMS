using Microsoft.EntityFrameworkCore;
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
