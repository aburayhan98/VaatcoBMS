using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaatcoBMS.Application.Interfaces;

namespace VaatcoBMS.Web.Controllers;

[Authorize]
[Route("Invoices")]
public class InvoicesController(
		IInvoiceService invoiceService,
		ICustomerService customerService,
		IProductService productService) : Controller
{
	private readonly IInvoiceService _invoiceService = invoiceService;
	private readonly ICustomerService _customerService = customerService;
	private readonly IProductService _productService = productService;

	[HttpGet("")]
	public async Task<IActionResult> Index()
	{
		var invoices = await _invoiceService.GetAllAsync(1, 200);
		return View(invoices);
	}

	[HttpGet("Create")]
	public IActionResult Create() => View();

	[HttpGet("{id:int}")]
	public async Task<IActionResult> Detail(int id)
	{
		var invoice = await _invoiceService.GetByIdAsync(id);
		if (invoice == null) return NotFound();
		return View(invoice);
	}
}
