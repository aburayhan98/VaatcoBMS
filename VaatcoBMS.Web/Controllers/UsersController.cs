using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaatcoBMS.Application.Interfaces;

namespace VaatcoBMS.Web.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")] // Only Admins can see the user list
public class UsersController(IUserService userService) : Controller
{
    private readonly IUserService _userService = userService;

    // This lists ALL users (Approved and Unapproved) or just Approved
    public async Task<IActionResult> Index()
    {
        var allUsers = await _userService.GetAllAsync();
        
        // Let's filter to only show APPROVED users here (pending users have their own page)
        var approvedUsers = allUsers.Where(u => u.IsApproved).ToList();
        
        return View(approvedUsers);
    }
}