using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaatcoBMS.Application.Interfaces;

namespace VaatcoBMS.Web.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")] // Only Admins can see the user list
public class UsersController(IUserService userService) : Controller
{
    private readonly IUserService _userService = userService;

	// Redirect old singular route to the new plural controller
	

	// GET /Users/Index
	public async Task<IActionResult> Index()
    {
        var allUsers = await _userService.GetAllAsync();
        return View("List", allUsers);
    }

    // GET /Users/List
    public async Task<IActionResult> List()
    {
        // Fetch ALL users unfiltered
        var allUsers = await _userService.GetAllAsync();
        return View(allUsers);
    }
}