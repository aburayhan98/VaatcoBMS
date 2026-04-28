using VaatcoBMS.Application.DTOs.Customer;
using VaatcoBMS.Application.DTOs.Product;
using VaatcoBMS.Domain.Common;

namespace VaatcoBMS.Application.Interfaces;

	public interface ICustomerService
	{
		Task<IEnumerable<CustomerDto>> GetAllAsync(); 
		Task<CustomerDto?> GetByIdAsync(int id); Task<CustomerDto?> GetByCodeAsync(string code); 
		Task<IEnumerable<CustomerDto>> SearchAsync(string keyword);
		//Task<IEnumerable<CustomerDto>> GetPagedAsync(int page, int pageSize); 
		Task<CustomerDto> CreateAsync(CreateCustomerDto dto); 
		Task<CustomerDto> UpdateAsync(int id, UpdateCustomerDto dto); 
		Task DeleteAsync(int id); // soft delete (IsActive = false)
	  Task<bool> PhoneExistsAsync(string phone, int? excludeId = null); 
	Task<int> GetTotalCountAsync();
	// <summary>Server-side paginated + filtered + sorted list.</summary>
	Task<PagedResult<CustomerDto>> GetPagedAsync(CustomerQueryParams q);
}
	
