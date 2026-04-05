
using Microsoft.EntityFrameworkCore;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Interfaces;
using VaatcoBMS.Infrastructure.Persistence;

namespace VaatcoBMS.Infrastructure.Repositories;

public class UserRepository(AppDbContext ctx) : Repository<User>(ctx), IUserRepository
{
	public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
	{
		return await _set.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
	}

	//public Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
	//{
	//	throw new NotImplementedException();
	//}

	public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
	{
		return await _set.AllAsync(u => u.Email != email, cancellationToken);
	}

	//public Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default)
	//{
	//	throw new NotImplementedException();
	//}
}
