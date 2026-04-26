using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaatcoBMS.Application.DTOs.Customer;
using VaatcoBMS.Application.Interfaces;

namespace VaatcoBMS.Web.Controllers.Api;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomerApiController(ICustomerService customerService) : ControllerBase
{
	private readonly ICustomerService _customerService = customerService;

	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var customers = await _customerService.GetAllAsync();
		return Ok(new { success = true, data = customers });
	}

	[HttpGet("{id:int}")]
	public async Task<IActionResult> GetById(int id)
	{
		var c = await _customerService.GetByIdAsync(id);
		if (c == null) return NotFound(new { success = false, message = "Customer not found." });
		return Ok(new { success = true, data = c });
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
