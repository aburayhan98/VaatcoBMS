using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaatcoBMS.Application.Interfaces;

namespace VaatcoBMS.Web.Controllers;

[Authorize] // Ensure only logged-in users can access this
public class CustomersController(ICustomerService customerService) : Controller
{
	private readonly ICustomerService _customerService = customerService;

	public async Task<IActionResult> Index()
	{
		// 1. Fetch all customers from the database via your application service
		var customers = await _customerService.GetAllAsync();

		// 2. Pass the list of CustomerDto to the Index.cshtml view
		return View(customers);
	}
}