

using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
	Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
	//Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
	Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
	//Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default);
}
