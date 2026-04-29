using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaatcoBMS.Application.DTOs.Customer;
using VaatcoBMS.Application.Interfaces;

namespace VaatcoBMS.Web.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class CustomersController(ICustomerService customerService) : Controller
{
	private readonly ICustomerService _customerService = customerService;

	// GET /Customers
	public async Task<IActionResult> Index()
	{
		var items = await _customerService.GetAllAsync();
		return View(items);
	}

	// GET /Customers/Create
	[HttpGet]
	public IActionResult Create()
	{
		return View(new CreateCustomerDto());
	}

	// POST /Customers/Create
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(CreateCustomerDto model)
	{
		if (!ModelState.IsValid) return View(model);

		try
		{
			var created = await _customerService.CreateAsync(model);
			TempData["Success"] = "Customer created successfully.";
			return RedirectToAction(nameof(Index));
		}
		catch (InvalidOperationException ex)
		{
			ModelState.AddModelError(string.Empty, ex.Message);
			return View(model);
		}
	}

	// GET /Customers/Edit/{id}
	[HttpGet]
	public async Task<IActionResult> Edit(int id)
	{
		var dto = await _customerService.GetByIdAsync(id);
		if (dto == null) return NotFound();

		var model = new UpdateCustomerDto
		{
			//Id = dto.Id,
			Name = dto.Name,
			ContactPerson = dto.ContactPerson,
			Address = dto.Address,
			City = dto.City,
			District = dto.District,
			Phone = dto.Phone,
			Email = dto.Email,
			IsActive = dto.IsActive
		};

		return View(model);
	}

	// POST /Customers/Edit/{id}
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(int id, UpdateCustomerDto model)
	{
		if (!ModelState.IsValid) return View(model);

		try
		{
			var updated = await _customerService.UpdateAsync(id, model);
			TempData["Success"] = "Customer updated.";
			return RedirectToAction(nameof(Index));
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(new { success = false, message = ex.Message });
		}
		catch (InvalidOperationException ex)
		{
			ModelState.AddModelError(string.Empty, ex.Message);
			return View(model);
		}
	}

	// GET /Customers/Details/{id}
	[HttpGet]
	public async Task<IActionResult> Details(int id)
	{
		var dto = await _customerService.GetByIdAsync(id);
		if (dto == null) return NotFound();
		return View(dto);
	}

	// POST /Customers/Delete/{id}
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			await _customerService.DeleteAsync(id);
			TempData["Success"] = "Customer deleted.";
			return RedirectToAction(nameof(Index));
		}
		catch (KeyNotFoundException) { return NotFound(); }
		catch (InvalidOperationException ex)
		{
			TempData["Error"] = ex.Message;
			return RedirectToAction(nameof(Index));
		}
	}
}