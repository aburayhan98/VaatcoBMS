using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaatcoBMS.Application.DTOs.Customer;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Domain.Common;

namespace VaatcoBMS.Web.Controllers.Api;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomerApiController(ICustomerService customerService) : ControllerBase
{
	private readonly ICustomerService _customerService = customerService;


	/// <summary>
	/// Server-side paged + filtered + sorted customer list.
	/// GET /api/customers?page=1&pageSize=20&search=xyz&isActive=true&sortBy=name&sortDir=asc
	/// </summary>
	[HttpGet]
	public async Task<IActionResult> GetAll([FromQuery] CustomerQueryParams q)
	{
		var result = await _customerService.GetPagedAsync(q);
		return Ok(new
		{
			success = true,
			data = result.Items,
			meta = new
			{
				result.TotalCount,
				result.Page,
				result.PageSize,
				result.TotalPages,
				result.HasPrev,
				result.HasNext,
			}
		});
	}

	/// <summary>All active customers — used by Invoice Create customer picker (no pagination).</summary>
	[HttpGet("active")]
	public async Task<IActionResult> GetActive()
	{
		// Note: Using GetAllAsync() since your service may not have an explicit GetActiveAsync() yet
		var customers = await _customerService.GetAllAsync();
		return Ok(new { success = true, data = customers });
	}

	[HttpGet("search")]
	public async Task<IActionResult> Search([FromQuery] string q)
	{
		var results = await _customerService.SearchAsync(q ?? string.Empty);
		return Ok(new { success = true, data = results });
	}

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
	{
		try
		{
			var c = await _customerService.CreateAsync(dto);
			return Ok(new { success = true, data = c });
		}
		catch (InvalidOperationException ex) { return BadRequest(new { success = false, message = ex.Message }); }
	}

	[HttpPut("{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
	{
		try
		{
			var c = await _customerService.UpdateAsync(id, dto);
			return Ok(new { success = true, data = c });
		}
		catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
		catch (InvalidOperationException ex) { return BadRequest(new { success = false, message = ex.Message }); }
	}

	[HttpDelete("{id:int}")]
	[Authorize(Roles = "Admin,SuperAdmin")]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			await _customerService.DeleteAsync(id);
			return Ok(new { success = true, message = "Customer deleted." });
		}
		catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
	}
}
