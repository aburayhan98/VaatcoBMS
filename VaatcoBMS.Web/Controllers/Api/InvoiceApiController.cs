using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VaatcoBMS.Application.DTOs.Invoice;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Domain.Enums;

namespace VaatcoBMS.Web.Controllers.Api;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoiceApiController(IInvoiceService invoiceService) : ControllerBase
{
	private readonly IInvoiceService _invoiceService = invoiceService;

	private int CallerId => int.TryParse(
			User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
			out var id) ? id : 0;

	[HttpGet]
	public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 20)
	{
		var invoices = await _invoiceService.GetAllAsync(page, size);
		return Ok(new { success = true, data = invoices });
	}

	[HttpGet("stats")]
	public async Task<IActionResult> GetStats()
	{
		var stats = await _invoiceService.GetStatsAsync();
		return Ok(new { success = true, data = stats });
	}

	[HttpGet("{id:int}")]
	public async Task<IActionResult> GetById(int id)
	{
		var invoice = await _invoiceService.GetByIdAsync(id);
		if (invoice == null)
			return NotFound(new { success = false, message = "Invoice not found." });
		return Ok(new { success = true, data = invoice });
	}

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateInvoiceDto dto)
	{
		try
		{
			var invoice = await _invoiceService.CreateAsync(dto, CallerId);
			return Ok(new { success = true, data = invoice, message = "Invoice created." });
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(new { success = false, message = ex.Message });
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(new { success = false, message = ex.Message });
		}
	}

	[HttpPut("{id:int}/status")]
	[Authorize(Roles = "Admin,SuperAdmin")]
	public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
	{
		try
		{
			if (!Enum.TryParse<InvoiceStatus>(status, out var parsed))
				return BadRequest(new { success = false, message = "Invalid status value." });
			await _invoiceService.UpdateStatusAsync(id, parsed);
			return Ok(new { success = true, message = $"Invoice status updated to {status}." });
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
			var result = await _invoiceService.DeleteAsync(id);
			if (!result) return NotFound(new { success = false, message = "Invoice not found." });
			return Ok(new { success = true, message = "Invoice deleted." });
		}
		catch (InvalidOperationException ex) { return BadRequest(new { success = false, message = ex.Message }); }
	}
}
