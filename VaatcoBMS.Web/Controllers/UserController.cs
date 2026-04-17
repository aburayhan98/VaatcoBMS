using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaatcoBMS.Application.Interfaces;

namespace VaatcoBMS.Web.Controllers
{
	[Authorize(Roles = "Admin,SuperAdmin")] // Only let Admins see the user list
	public class UserController(IUserService userService) : Controller
	{
		private readonly IUserService _userService = userService;

		// GET /User/Index
		public async Task<IActionResult> Index()
		{
			// Fetch all users
			var allUsers = await _userService.GetAllAsync();
			
			// If you only want approved users shown here:
			// var approvedUsers = allUsers.Where(u => u.IsApproved).ToList();
			// return View(approvedUsers);
			
			return View(allUsers);
		}
	}
}
