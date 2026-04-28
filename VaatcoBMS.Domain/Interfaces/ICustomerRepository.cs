using VaatcoBMS.Domain.Common;
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
	Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
	Task<Customer?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
	Task<Customer?> GetByCustomerAsync(string ContactPerson, CancellationToken cancellationToken = default);
	Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
	Task<IEnumerable<Customer>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
	//GetByCodeAsync
	/// <summary>
	/// Server-side filtered, sorted, paged query.
	/// Single round-trip: COUNT + SELECT in one EF execution.
	/// </summary>
	Task<PagedResult<Customer>> GetPagedAsync(CustomerQueryParams q);
}

