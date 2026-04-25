using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VaatcoBMS.Application.DTOs.User;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Domain.Enums;

namespace VaatcoBMS.Web.Controllers.Api;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserApiController(IUserService userService) : ControllerBase
{
	private readonly IUserService _userService = userService;

	private int CallerId => int.TryParse(
			User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
			out var id) ? id : 0;

	/// <summary>
	/// Get all users (Admin/SuperAdmin only).
	/// </summary>
	[HttpGet]
	[Authorize(Roles = "Admin,SuperAdmin")]
	public async Task<IActionResult> GetAll()
	{
		var users = await _userService.GetAllAsync();
		return Ok(new { success = true, data = users });
	}

	/// <summary>
	/// Get users pending approval.
	/// </summary>
	[HttpGet("pending")]
	[Authorize(Roles = "Admin,SuperAdmin")]
	public async Task<IActionResult> GetPending()
	{
		var users = await _userService.GetPendingApprovalAsync();
		return Ok(new { success = true, data = users });
	}

	/// <summary>
	/// Approve a user. SuperAdmin required to approve Admins.
	/// </summary>
	[HttpPost("{id}/approve")]
	[Authorize(Roles = "Admin,SuperAdmin")]
	public async Task<IActionResult> Approve(int id)
	{
		try
		{
			await _userService.ApproveUserAsync(id, CallerId);
			return Ok(new { success = true, message = "User approved." });
		}
		catch (UnauthorizedAccessException ex)
		{
			return StatusCode(403, new { success = false, message = ex.Message });
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(new { success = false, message = ex.Message });
		}
	}

	/// <summary>
	/// Reject and delete a pending user.
	/// </summary>
	[HttpPost("{id}/reject")]
	[Authorize(Roles = "Admin,SuperAdmin")]
	public async Task<IActionResult> Reject(int id)
	{
		try
		{
			await _userService.RejectUserAsync(id);
			return Ok(new { success = true, message = "User rejected." });
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(new { success = false, message = ex.Message });
		}
	}

	/// <summary>
	/// Deactivate (soft-disable) a user account.
	/// </summary>
	[HttpPost("{id}/deactivate")]
	[Authorize(Roles = "Admin,SuperAdmin")]
	public async Task<IActionResult> Deactivate(int id)
	{
		try
		{
			await _userService.DeactivateAsync(id);
			return Ok(new { success = true, message = "User deactivated." });
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(new { success = false, message = ex.Message });
		}
	}

	/// <summary>
	/// Change role of a user (SuperAdmin only).
	/// </summary>
	[HttpPost("{id}/change-role")]
	[Authorize(Roles = "SuperAdmin")]
	public async Task<IActionResult> ChangeRole(int id, [FromBody] UserRole newRole)
	{
		try
		{
			await _userService.ChangeRoleAsync(id, newRole);
			return Ok(new { success = true, message = $"Role changed to {newRole}." });
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(new { success = false, message = ex.Message });
		}
	}

	/// <summary>
	/// Update own profile.
	/// </summary>
	[HttpPut("{id}/profile")]
	public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileDto dto)
	{
		try
		{
			var result = await _userService.UpdateProfileAsync(id, dto);
			return Ok(new { success = true, data = result });
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

	/// <summary>
	/// Change password.
	/// </summary>
	[HttpPost("{id}/change-password")]
	public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
	{
		try
		{
			await _userService.ChangePasswordAsync(id, dto);
			return Ok(new { success = true, message = "Password changed successfully." });
		}
		catch (UnauthorizedAccessException ex)
		{
			return StatusCode(403, new { success = false, message = ex.Message });
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(new { success = false, message = ex.Message });
		}
	}

	/// <summary>
	/// Manually resets a user's password (SuperAdmin only).
	/// </summary>
	[HttpPost("{id}/admin-reset-password")]
	[Authorize(Roles = "SuperAdmin")]
	public async Task<IActionResult> AdminResetPassword(int id, [FromBody] AdminResetPasswordDto dto)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(dto.NewPassword))
				return BadRequest(new { success = false, message = "Password cannot be empty." });

			await _userService.AdminResetUserPasswordAsync(id, dto.NewPassword, CallerId);
			return Ok(new { success = true, message = "Password was reset successfully for account." });
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(new { success = false, message = ex.Message });
		}
		catch (UnauthorizedAccessException ex)
		{
			return StatusCode(403, new { success = false, message = ex.Message });
		}
	}
}