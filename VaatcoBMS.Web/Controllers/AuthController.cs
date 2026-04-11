using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Application.Model.Auth;
using VaatcoBMS.Infrastructure.Services;

namespace VaatcoBMS.Web.Controllers;

[Route("auth")]
public class AuthController(
	IAuthService authService,
	IEmailService emailService) : Controller
{
	private readonly IAuthService _authService = authService;
	private readonly IEmailService _emailService = emailService; // Initialize the email service

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

			// set secure HttpOnly cookies
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
	[AllowAnonymous] // clients call with cookie
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
				Expires = tokens.ExpiresAtUtc.AddDays(7) // refresh token also gets extended
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
			var token = await _authService.RegisterAsync(model); // now returns token
			var link = Url.Action("VerifyEmail", "Auth", new { token }, Request.Scheme, Request.Host.Value);
			var body = $"<p>Hi {model.Name},</p><p>Verify your email: <a href='{link}'>Click here</a></p>";

			await _emailService.SendEmailAsync(model.Email, "Verify Your Account", body);

			TempData["Success"] = "Account created. Check email to verify before logging in.";
			return RedirectToAction("Index", "Home"); // as you requested: go to Index after registration
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
		// 1. Delete the secure authentication cookies
		Response.Cookies.Delete("access_token");
		Response.Cookies.Delete("refresh_token");

		// 2. Set a nice logout message
		TempData["Success"] = "You have been successfully logged out.";

		// 3. Redirect back to the auth/login page
		return RedirectToAction("Login");
	}

	// Default fallback action
	[HttpGet("index")]
	public IActionResult Index()
	{
		return View();
	}

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
			// Call your existing service method to validate the token and update the database
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
}

