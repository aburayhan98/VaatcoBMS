using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaatcoBMS.Application.DTOs.Product;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Domain.Common;

namespace VaatcoBMS.Web.Controllers.Api;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductApiController(IProductService productService) : ControllerBase
{
	private readonly IProductService _productService = productService;

	/// <summary>
	/// Server-side paged + filtered + sorted product list.
	/// GET /api/products?page=1&pageSize=20&search=xyz&stockStatus=Low&isActive=true&sortBy=name&sortDir=asc
	/// </summary>
	[HttpGet]
	public async Task<IActionResult> GetAll([FromQuery] ProductQueryParams q)
	{
		var result = await _productService.GetPagedAsync(q);
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

	/// <summary>All active products — used by Invoice Create product picker (no pagination).</summary>
	[HttpGet("active")]
	public async Task<IActionResult> GetActive()
	{
		var products = await _productService.GetActiveAsync();
		return Ok(new { success = true, data = products });
	}

	/// <summary>Products at or below the given stock threshold.</summary>
	[HttpGet("low-stock")]
	public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 20)
	{
		var products = await _productService.GetLowStockAsync(threshold);
		return Ok(new { success = true, data = products });
	}

	[HttpGet("{id:int}")]
	public async Task<IActionResult> GetById(int id)
	{
		var p = await _productService.GetByIdAsync(id);
		if (p is null) return NotFound(new { success = false, message = "Product not found." });
		return Ok(new { success = true, data = p });
	}

	[HttpGet("search")]
	public async Task<IActionResult> Search([FromQuery] string q)
	{
		var results = await _productService.SearchAsync(q ?? string.Empty);
		return Ok(new { success = true, data = results });
	}

	[HttpPost]
	[Authorize(Roles = "Admin,SuperAdmin")]
	public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
	{
		try
		{
			var p = await _productService.CreateAsync(dto);
			return Ok(new { success = true, data = p });
		}
		catch (InvalidOperationException ex) { return BadRequest(new { success = false, message = ex.Message }); }
	}

	[HttpPut("{id:int}")]
	[Authorize(Roles = "Admin,SuperAdmin")]
	public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
	{
		try
		{
			var p = await _productService.UpdateAsync(id, dto);
			return Ok(new { success = true, data = p });
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
			await _productService.DeleteAsync(id);
			return Ok(new { success = true, message = "Product deleted." });
		}
		catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
	}
}
