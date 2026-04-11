using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Application.Model.Auth;

namespace VaatcoBMS.Web.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthApiController(IAuthService authService) : ControllerBase
{

	/// <summary>
	/// Admin or SuperAdmin creates a new user account directly.
	/// Bypasses email verification and approval workflow.
	/// </summary>
	[HttpPost("admin-create")]
	[Authorize(Roles = "Admin,SuperAdmin")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> AdminCreateUser([FromBody] Register model)
	{
		try
		{
			var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

			if (!int.TryParse(userIdClaim, out int callerId))
				return Unauthorized(new { success = false, message = "Cannot identify caller." });

			var user = await authService.CreateUserByAdminAsync(model, callerId);
			return Ok(new { success = true, data = user, message = "User created successfully." });
		}
		catch (UnauthorizedAccessException ex)
		{
			return StatusCode(403, new { success = false, message = ex.Message });
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(new { success = false, message = ex.Message });
		}
	}
	/// <summary>
	/// Authenticates a user and returns JWT tokens.
	/// </summary>
	[HttpPost("login")]
	[AllowAnonymous]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Login([FromBody] LoginModel model)
	{
		try
		{
			var tokens = await authService.LoginAsync(model);
			return Ok(new { success = true, data = tokens });
		}
		catch (UnauthorizedAccessException ex)
		{
			return Unauthorized(new { success = false, message = ex.Message });
		}
	}

	/// <summary>
	/// Registers a new user account.
	/// </summary>
	[HttpPost("register")]
	[AllowAnonymous]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> Register([FromBody] Register model)
	{
		try
		{
			var user = await authService.RegisterAsync(model);
			return Ok(new
			{
				success = true,
				data = user,
				message = "Registration successful. Please verify your email."
			});
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(new { success = false, message = ex.Message });
		}
	}

	/// <summary>
	/// Refreshes an expired JWT access token using a valid refresh token.
	/// </summary>
	[HttpPost("refresh")]
	[AllowAnonymous]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public IActionResult Refresh([FromBody] string refreshToken)
	{
		try
		{
			var tokens = authService.RefreshLogin(refreshToken);
			return Ok(new { success = true, data = tokens });
		}
		catch (UnauthorizedAccessException ex)
		{
			return Unauthorized(new { success = false, message = ex.Message });
		}
	}

	/// <summary>
	/// Verifies a user's email address via token.
	/// </summary>
	[HttpGet("verify-email")]
	[AllowAnonymous]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> VerifyEmail([FromQuery] string token)
	{
		var result = await authService.VerifyEmailAsync(token);

		return result
				? Ok(new { success = true, message = "Email verified." })
				: BadRequest(new { success = false, message = "Invalid token." });
	}

	/// <summary>
	/// Initiates the forgot password workflow.
	/// </summary>
	[HttpPost("forgot-password")]
	[AllowAnonymous]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
	{
		await authService.ForgotPasswordAsync(model.Email);

		// Always return OK to prevent email enumeration attacks
		return Ok(new
		{
			success = true,
			message = "If that email exists a reset link has been sent."
		});
	}

	/// <summary>
	/// Resets a user's password using a secure token.
	/// </summary>
	[HttpPost("reset-password")]
	[AllowAnonymous]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
	{
		if (model.Password != model.ConfirmPassword)
		{
			return BadRequest(new { success = false, message = "Passwords do not match." });
		}

		await authService.ResetPasswordAsync(model);
		return Ok(new { success = true, message = "Password reset successfully." });
	}

	/// <summary>
	/// Retrieves claims/details for the currently authenticated user.
	/// </summary>
	[HttpGet("me")]
	[Authorize] // 👈 Requires a valid JWT Bearer token
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public IActionResult Me()
	{
		var claims = User.Claims.Select(c => new { c.Type, c.Value });
		return Ok(new { success = true, data = claims });
	}
}