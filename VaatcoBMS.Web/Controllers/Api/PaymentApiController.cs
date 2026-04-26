using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaatcoBMS.Application.DTOs.Payment;
using VaatcoBMS.Application.Interfaces;

namespace VaatcoBMS.Web.Controllers.Api;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentApiController(IPaymentService paymentService) : ControllerBase
{
	private readonly IPaymentService _paymentService = paymentService;

	[HttpGet("invoice/{invoiceId:int}")]
	public async Task<IActionResult> GetByInvoice(int invoiceId)
	{
		var payments = await _paymentService.GetByInvoiceAsync(invoiceId);
		return Ok(new { success = true, data = payments });
	}

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
	{
		try
		{
			var payment = await _paymentService.CreateAsync(dto);
			return Ok(new { success = true, data = payment, message = "Payment recorded." });
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
			await _paymentService.DeleteAsync(id);
			return Ok(new { success = true, message = "Payment deleted." });
		}
		catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
	}
}
