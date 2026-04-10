using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Application.Model.Auth;

namespace VaatcoBMS.Web.Controllers;

[Route("auth")]
public class AuthController(IAuthService authService, IUserService userService) : Controller
{
	private readonly IAuthService _authService = authService;
	private readonly IUserService _userService = userService;

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
			await _authService.RegisterAsync(model);
			TempData["Success"] = "Registration successful! Please log in.";
			return RedirectToAction("Login");
		}
		catch (Exception ex)
		{
			ModelState.AddModelError(string.Empty, ex.Message);
			return View(model);
		}
	}

	// Default fallback action
	[HttpGet("index")]
	public IActionResult Index()
	{
		return View();
	}
}

