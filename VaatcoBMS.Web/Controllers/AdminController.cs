using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Application.Model.Auth;

namespace VaatcoBMS.Web.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
[Route("admin")]
public class AdminController(IUserService userService, IAuthService authService) : Controller
{
	private readonly IUserService _userService = userService;
	private readonly IAuthService _authService = authService;

	// GET /admin/users — list all users
	[HttpGet("users")]
	public async Task<IActionResult> Users()
	{
		var users = await _userService.GetAllAsync();
		return View(users);
	}

	// GET /admin/pending — users waiting approval
	[HttpGet("pending")]
	public async Task<IActionResult> PendingApproval()
	{
		var pending = await _userService.GetPendingApprovalAsync();
		return View(pending);
	}

	// GET /admin/create-user — form to manually create account
	[HttpGet("create-user")]
	public IActionResult CreateUser()
	{
		return View(new Register());
	}

	// POST /admin/create-user
	[HttpPost("create-user")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> CreateUser(Register model)
	{
		if (!ModelState.IsValid)
		{
			return View(model);
		}

		try
		{
			var callerIdStr = User.Claims
					.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)
					?.Value;

			_ = int.TryParse(callerIdStr, out int callerId);

			await _authService.CreateUserByAdminAsync(model, callerId);

			TempData["Success"] = "User created successfully.";
			return RedirectToAction("Users");
		}
		catch (Exception ex)
		{
			ModelState.AddModelError(string.Empty, ex.Message);
			return View(model);
		}
	}

	// ── SuperAdmin-only section ───────────────────────────────────────

	// GET /admin/manage-admins — list and approve pending Admin accounts
	[Authorize(Roles = "SuperAdmin")]
	[HttpGet("manage-admins")]
	public async Task<IActionResult> ManageAdmins()
	{
		var all = await _userService.GetAllAsync();
		var admins = all.Where(u => u.Role.ToString() == "Admin");
		return View(admins);
	}
}