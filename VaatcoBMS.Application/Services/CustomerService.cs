
using MapsterMapper;
using Microsoft.Extensions.Logging;
using VaatcoBMS.Application.DTOs.Customer;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Domain.Common;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Interfaces;

namespace VaatcoBMS.Application.Services;

public class CustomerService(
	IUnitOfWork uow,
	IMapper mapper, 
	ILogger<CustomerService> logger) : ICustomerService
{
	private readonly IUnitOfWork _uow = uow;
	private readonly IMapper _mapper = mapper;
	private readonly ILogger<CustomerService> _logger = logger;

	public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
	{
		try
		{
			if (await PhoneExistsAsync(dto.Phone)) 
			{
				_logger.LogWarning("Attempted to create a customer with an existing phone number: {Phone}", dto.Phone);
				throw new InvalidOperationException($"A customer with phone {dto.Phone} already exists."); 
			}
				
			var customer = _mapper.Map<Customer>(dto);
			customer.CreatedAt = DateTime.UtcNow;
			customer.IsActive = true; 
		
			await _uow.Customers.AddAsync(customer);
			await _uow.SaveChangesAsync();
			
			_logger.LogInformation("Successfully created customer with phone {Phone}", dto.Phone);
			return _mapper.Map<CustomerDto>(customer);
		}
		catch (Exception ex) when (ex is not InvalidOperationException)
		{
			_logger.LogError(ex, "An error occurred while creating the customer.");
			throw new ApplicationException("An error occurred while saving the customer to the database.", ex);
		}
	}

	public async Task DeleteAsync(int id)
	{
		try
		{
			var customer = await _uow.Customers.GetByIdAsync(id) 
			            ?? throw new KeyNotFoundException($"Customer {id} not found.");
			            
			customer.IsActive = false; // soft delete — never hard delete
			_uow.Customers.Update(customer);
			await _uow.SaveChangesAsync();
			
			_logger.LogInformation("Successfully soft-deleted customer {Id}", id);
		}
		catch (Exception ex) when (ex is not KeyNotFoundException)
		{
			_logger.LogError(ex, "An error occurred while deleting customer {Id}", id);
			throw new ApplicationException($"An error occurred while deleting customer {id}.", ex);
		}
	}

	public async Task<IEnumerable<CustomerDto>> GetAllAsync()
	{
		try
		{
			var customers = await _uow.Customers.GetAllAsync();
			return _mapper.Map<IEnumerable<CustomerDto>>(customers.Where(c => c.IsActive));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while fetching all customers.");
			throw new ApplicationException("An error occurred while fetching customers.", ex);
		}
	}

	public async Task<CustomerDto?> GetByCodeAsync(string code)
	{
		try
		{
			var customers = await _uow.Customers.GetAllAsync();
			var customer = customers.FirstOrDefault(c => c.CustomerCode == code);
			
			if (customer == null || !customer.IsActive)
			{
				return null;
			}
			
			return _mapper.Map<CustomerDto>(customer);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while fetching customer by code {Code}", code);
			throw new ApplicationException($"An error occurred while fetching customer {code}.", ex);
		}
	}

	public async Task<CustomerDto?> GetByIdAsync(int id) 
	{ 
		try
		{
			var customer = await _uow.Customers.GetByIdAsync(id);
			
			if (customer == null || !customer.IsActive)
			{
				return null;
			}

			return _mapper.Map<CustomerDto>(customer); 
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while fetching customer {Id}", id);
			throw new ApplicationException($"An error occurred while fetching customer {id}.", ex);
		}
	}

	public async Task<PagedResult<CustomerDto>> GetPagedAsync(CustomerQueryParams q)
	{
		var result = await _uow.Customers.GetPagedAsync(q);
		return new PagedResult<CustomerDto>
		{
			Items = _mapper.Map<IEnumerable<CustomerDto>>(result.Items),
			TotalCount = result.TotalCount,
			Page = result.Page,
			PageSize = result.PageSize,
		};
	}

	public async Task<int> GetTotalCountAsync() 
	{
		try
		{
			var customers = await _uow.Customers.GetAllAsync();
			return customers.Count(c => c.IsActive);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while getting total customer count.");
			throw new ApplicationException("An error occurred while counting customers.", ex);
		}
	}

	public async Task<bool> PhoneExistsAsync(string phone, int? excludeId = null) 
	{ 
		try
		{
			var all = await _uow.Customers.GetAllAsync(); 
			return all.Any(c => c.Phone == phone && 
			                    c.IsActive && 
			                    (excludeId == null || c.Id != excludeId)); 
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while checking if phone {Phone} exists.", phone);
			throw new ApplicationException("An error occurred while validating the phone number.", ex);
		}
	}

	public async Task<IEnumerable<CustomerDto>> SearchAsync(string keyword)
	{
		try
		{
			var all = await _uow.Customers.GetAllAsync();

			var filtered = all.Where(c =>
				c.IsActive &&
				(
					(c.Name?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true) ||
					(c.Phone?.Contains(keyword) == true) ||
					(c.CustomerCode?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true)
				)
			);

			return _mapper.Map<IEnumerable<CustomerDto>>(filtered);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while searching for customers with keyword {Keyword}", keyword);
			throw new ApplicationException("An error occurred while searching customers.", ex);
		}
	}

	public async Task<CustomerDto> UpdateAsync(int id, UpdateCustomerDto dto)
	{
		try
		{
			var customer = await _uow.Customers.GetByIdAsync(id) 
			               ?? throw new KeyNotFoundException($"Customer {id} not found."); 
			                
			if (await PhoneExistsAsync(dto.Phone, id)) 
			{
				_logger.LogWarning("Attempted to update customer {Id} with an existing phone number {Phone}", id, dto.Phone);
				throw new InvalidOperationException("Another customer already uses this phone number."); 
			}
				
			// map onto existing entity
			_mapper.Map(dto, customer); 
			
			_uow.Customers.Update(customer);
			await _uow.SaveChangesAsync(); 
			
			_logger.LogInformation("Successfully updated customer {Id}", id);
			return _mapper.Map<CustomerDto>(customer);
		}
		catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
		{
			_logger.LogError(ex, "An error occurred while updating customer {Id}", id);
			throw new ApplicationException($"An error occurred while updating customer {id}.", ex);
		}
	}
}
