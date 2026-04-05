using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
	Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
	Task<Customer?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
	Task<Customer?> GetByCustomerAsync(string ContactPerson, CancellationToken cancellationToken = default);
	Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
	Task<IEnumerable<Customer>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}

