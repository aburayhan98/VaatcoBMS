using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Application.Model.Auth;

namespace VaatcoBMS.Web.Controllers;

[Route("auth")]
public class AuthController(
	IAuthService authService,
	IEmailService emailService) : Controller
{
	private readonly IAuthService _authService = authService;
	private readonly IEmailService _emailService = emailService;

	// GET /auth/login
	[HttpGet("login")]
	[AllowAnonymous]
	public IActionResult Login(string? returnUrl = null)
	{
		if (User.Identity?.IsAuthenticated == true)
			return RedirectToAction("Index", "Home");

		ViewData["ReturnUrl"] = returnUrl;
		return View();
	}

	// POST /auth/login
	[HttpPost("login")]
	[AllowAnonymous]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
	{
		if (!ModelState.IsValid)
			return View(model);

		try
		{
			var tokens = await _authService.LoginAsync(model);

			Response.Cookies.Append("access_token", tokens.AccessToken, new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = tokens.ExpiresAtUtc
			});

			Response.Cookies.Append("refresh_token", tokens.RefreshToken, new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = DateTimeOffset.UtcNow.AddDays(7)
			});

			TempData["Success"] = "Welcome back!";
			return LocalRedirect(returnUrl ?? Url.Action("Index", "Home") ?? "/");
		}
		catch (UnauthorizedAccessException ex)
		{
			ModelState.AddModelError(string.Empty, ex.Message);
			return View(model);
		}
	}

	// POST /auth/refresh
	[HttpPost("refresh")]
	[AllowAnonymous]
	[ValidateAntiForgeryToken]
	public IActionResult Refresh()
	{
		var refresh = Request.Cookies["refresh_token"];
		if (string.IsNullOrEmpty(refresh)) return Unauthorized();

		try
		{
			var tokens = _authService.RefreshLogin(refresh);
			Response.Cookies.Append("access_token", tokens.AccessToken, new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = tokens.ExpiresAtUtc.AddDays(7)
			});
			Response.Cookies.Append("refresh_token", tokens.RefreshToken, new CookieOptions
			{
				HttpOnly = true,
			 Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = tokens.ExpiresAtUtc.AddDays(7)
			});
			return Ok();
		}
		catch
		{
			Response.Cookies.Delete("access_token");
			Response.Cookies.Delete("refresh_token");
			return Unauthorized();
		}
	}

	// GET /auth/register
	[HttpGet("register")]
	[AllowAnonymous]
	public IActionResult Register() => View();

	// POST /auth/register
	[HttpPost("register")]
	[AllowAnonymous]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Register(Register model)
	{
		if (!ModelState.IsValid)
			return View(model);

		try
		{
			var token = await _authService.RegisterAsync(model);
			var link = Url.Action("VerifyEmail", "Auth", new { token }, Request.Scheme, Request.Host.Value);
			var body = $"<p>Hi {model.Name},</p><p>Verify your email: <a href='{link}'>Click here</a></p>";

			await _emailService.SendEmailAsync(model.Email, "Verify Your Account", body);

			TempData["Success"] = "Account created. Check email to verify before logging in.";
			return RedirectToAction("Index", "Home");
		}
		catch (InvalidOperationException ex)
		{
			ModelState.AddModelError("Email", ex.Message);
			return View(model);
		}
		catch (Exception)
		{
			ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
			return View(model);
		}
	}

	// GET /auth/logout
	[HttpGet("logout")]
	[AllowAnonymous]
	public IActionResult Logout()
	{
		Response.Cookies.Delete("access_token");
		Response.Cookies.Delete("refresh_token");

		TempData["Success"] = "You have been successfully logged out.";
		return RedirectToAction("Login");
	}

	// Default fallback action
	[HttpGet("index")]
	public IActionResult Index() => View();

	// GET /auth/verify-email
	[HttpGet("VerifyEmail")]
	[AllowAnonymous]
	public async Task<IActionResult> VerifyEmail(string token)
	{
		if (string.IsNullOrEmpty(token))
		{
			TempData["Error"] = "Verification token is missing or invalid.";
			return RedirectToAction("Login");
		}

		try
		{
			var isSuccess = await _authService.VerifyEmailAsync(token);

			if (isSuccess)
			{
				TempData["Success"] = "Your email has been successfully verified! You can now log in.";
			}
			else
			{
				TempData["Error"] = "Email verification failed or token has expired. Please try registering again.";
			}
		}
		catch (Exception)
		{
			TempData["Error"] = "An unexpected error occurred during email verification. Please try again.";
		}

		return RedirectToAction("Login");
	}

	// GET /auth/forgot-password
	[HttpGet("forgot-password")]
	[AllowAnonymous]
	public IActionResult ForgotPassword() => View(new ForgotPasswordModel());

	// POST /auth/forgot-password
	[HttpPost("forgot-password")]
	[AllowAnonymous]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
	{
		if (!ModelState.IsValid)
			return View(model);

		// Sending the email request (Silent response to prevent user enumeration)
		await _authService.ForgotPasswordAsync(model.Email);

		TempData["Success"] = "If an account exists with that email, a password reset link has been sent.";
		return RedirectToAction("Login");
	}

	// GET /auth/reset-password
	[HttpGet("reset-password")]
	[AllowAnonymous]
	public IActionResult ResetPassword(string token)
	{
		if (string.IsNullOrEmpty(token))
		{
			TempData["Error"] = "A valid token must be supplied for password reset.";
			return RedirectToAction("Login");
		}

		var model = new ResetPasswordModel { ResetToken = token }; // Map directly to new property name
		return View(model);
	}

	// POST /auth/reset-password
	[HttpPost("reset-password")]
	[AllowAnonymous]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
	{
		if (!ModelState.IsValid)
			return View(model);

		try
		{
			await _authService.ResetPasswordAsync(model);

			// Updated: Custom message to let them know they must wait for admin approval
			TempData["Success"] = "Your password has been reset successfully. Please wait for Admin approval before logging in.";

			return RedirectToAction("Login"); // Redirects them to the login page (GET /auth/login)
		}
		catch (InvalidOperationException ex)
		{
			ModelState.AddModelError(string.Empty, ex.Message);
			return View(model);
		}
	}
}

