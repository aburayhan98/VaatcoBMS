using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using VaatcoBMS.Application;
using VaatcoBMS.Application.DTOs.User;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Application.Model.Auth;
using VaatcoBMS.Domain.Entities;
using VaatcoBMS.Domain.Enums;
using VaatcoBMS.Domain.Interfaces;
using VaatcoBMS.Infrastructure.Utility;

namespace VaatcoBMS.Infrastructure.Services;

public class AuthService(
		IUnitOfWork uow,
		IMapper mapper,
		ITokenBuilder tokenBuilder,
		IEmailService emailService,
		ILogger<AuthService> logger,
		IConfiguration configuration,
		IHttpContextAccessor httpContextAccessor) : IAuthService // INJECTED IHttpContextAccessor HERE!
{
	private readonly IUnitOfWork _uow = uow;
	private readonly IMapper _mapper = mapper;
	private readonly ITokenBuilder _tokenBuilder = tokenBuilder;
	private readonly IEmailService _emailService = emailService;
	private readonly ILogger<AuthService> _logger = logger;
	private readonly IConfiguration _configuration = configuration;
	private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

	// Helper to ALWAYS get the correct running URL dynamically
	private string GetBaseUrl()
	{
		var request = _httpContextAccessor.HttpContext?.Request;
		if (request != null)
		{
			// This perfectly builds "https://localhost:7169" (or whatever port you are actually using)
			return $"{request.Scheme}://{request.Host.Value}";
		}
		
		// Fallback for background tasks where HTTP context is lost
		return _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7169";
	}

	// ── LOGIN ────────────────────────────────────────────────────────
	public async Task<TokenResponse> LoginAsync(LoginModel model)
	{
		var users = await _uow.Users.GetAllAsync();
		var user = users.FirstOrDefault(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase));

		if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
		{
			_logger.LogWarning("Failed login for: {Email}", model.Email);
			throw new UnauthorizedAccessException("Invalid email or password.");
		}

		if (!user.IsApproved)
		{
			throw new UnauthorizedAccessException("Account is pending approval.");
		}

		_logger.LogInformation("Login: {Email}", user.Email);

		return _tokenBuilder.BuildTokens(
				user.Email,
				user.Id,
				user.Name,
				user.Role.ToString());
	}

	// ── REFRESH ──────────────────────────────────────────────────────
	public TokenResponse RefreshLogin(string refreshToken)
	{
		var tokens = _tokenBuilder.RefreshTokens(refreshToken);
		return tokens ?? throw new UnauthorizedAccessException("Invalid refresh token.");
	}

	// ── SELF-REGISTER (public) ────────────────────────────────────────
	public async Task<string> RegisterAsync(Register model)
	{
		if (model.Role == UserRole.Admin || model.Role == UserRole.SuperAdmin)
		{
			throw new InvalidOperationException("Admin and SuperAdmin accounts cannot be self-registered.");
		}

		var users = await _uow.Users.GetAllAsync();
		var existingUser = users.FirstOrDefault(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase));

		User userToMap;

		if (existingUser != null)
		{
			// If the user already exists and successfully verified their email, block them.
			if (existingUser.EmailConfirmed)
			{
				throw new InvalidOperationException("Email is already registered.");
			}

			// If they exist but are UNVERIFIED, let them "re-register".
			// Update their details in case they made a typo in their password or name previously.
			existingUser.Name = model.Name;
			existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
			existingUser.CreatedAt = DateTime.UtcNow; // Reset their timer
			
			_uow.Users.Update(existingUser);
			userToMap = existingUser;
		}
		else
		{
			// Brand new user
			userToMap = _mapper.Map<User>(model);
			userToMap.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
			userToMap.IsApproved = false;
			userToMap.EmailConfirmed = false;
			userToMap.CreatedAt = DateTime.UtcNow;

			await _uow.Users.AddAsync(userToMap);
		}

		// ALWAYS save to DB first to ensure we have an ID and updated state
		await _uow.SaveChangesAsync();

		// Email verification token generation
		var token = _tokenBuilder.BuildEmailToken(userToMap, "email_verification");

		try
		{
			var baseUrl = GetBaseUrl(); 
			var link = $"{baseUrl}/auth/verify-email?token={Uri.EscapeDataString(token)}";
			var body = $"<p>Hi {userToMap.Name},</p>" +
					   $"<p>Verify your email: <a href='{link}'>Click Here</a></p>";

			await _emailService.SendEmailAsync(userToMap.Email, "Verify Your Account", body);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send verification email to {Email}", userToMap.Email);
			throw new InvalidOperationException("Registration details saved, but failed to send verification email. Please try registering again to resend the link.");
		}

		return token;
	}

	// ── ADMIN-CREATED ACCOUNT (bypasses email verify & approval) ─────
	public async Task<UserDto> CreateUserByAdminAsync(Register model, int createdByUserId)
	{
		var creator = await _uow.Users.GetByIdAsync(createdByUserId);

		if (creator == null || (creator.Role != UserRole.Admin && creator.Role != UserRole.SuperAdmin))
		{
			throw new UnauthorizedAccessException("Insufficient privileges.");
		}

		// Admin cannot create Admin or SuperAdmin
		if (creator.Role == UserRole.Admin && (model.Role == UserRole.Admin || model.Role == UserRole.SuperAdmin))
		{
			throw new UnauthorizedAccessException("Admins cannot create Admin or SuperAdmin accounts.");
		}

		var users = await _uow.Users.GetAllAsync();

		if (users.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
		{
			throw new InvalidOperationException("Email already registered.");
		}

		var user = _mapper.Map<User>(model);
		user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
		user.IsApproved = true; // skip approval
		user.EmailConfirmed = true; // skip email verification
		user.CreatedAt = DateTime.UtcNow;

		await _uow.Users.AddAsync(user);
		await _uow.SaveChangesAsync();

		_logger.LogInformation("User {Email} created by admin {CreatorId}", user.Email, createdByUserId);

		return _mapper.Map<UserDto>(user);
	}

	// ── VERIFY EMAIL ─────────────────────────────────────────────────
	public async Task<bool> VerifyEmailAsync(string token)
	{
		if (!_tokenBuilder.IsJwtValid(token))
		{
			return false;
		}

		var claims = _tokenBuilder.GetClaims(token);
		var emailClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

		if (string.IsNullOrEmpty(emailClaim))
		{
			return false;
		}

		var users = await _uow.Users.GetAllAsync();
		var user = users.FirstOrDefault(u => u.Email.Equals(emailClaim, StringComparison.OrdinalIgnoreCase));

		if (user == null)
		{
			return false;
		}

		user.EmailConfirmed = true;
		_uow.Users.Update(user);
		await _uow.SaveChangesAsync();

		return true;
	}

	// ── FORGOT PASSWORD ──────────────────────────────────────────────
	public async Task ForgotPasswordAsync(string email)
	{
		var users = await _uow.Users.GetAllAsync();
		var user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

		if (user == null)
		{
			return; // silent — no email enumeration
		}

		try
		{
			var token = _tokenBuilder.BuildEmailToken(user, "password_reset");
			var baseUrl = GetBaseUrl(); // Call the dynamic URL helper!
			var link = $"{baseUrl}/auth/reset-password?token={Uri.EscapeDataString(token)}";
			var body = $"<p>Hi {user.Name},</p>" +
								 $"<p>Reset your password: <a href='{link}'>Click Here</a></p>";

			await _emailService.SendEmailAsync(user.Email, "Reset Your Password", body);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Password reset email failed for {Email}", email);
		}
	}

	public async Task ResetPasswordAsync(ResetPasswordModel model)
	{
		// 1. Validate Token
		if (!_tokenBuilder.IsJwtValid(model.ResetToken))
		{
			throw new InvalidOperationException("Invalid or expired password reset token.");
		}

		// 2. Extract Claims
		var claims = _tokenBuilder.GetClaims(model.ResetToken);
		var purposeClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
		var emailClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

		if (purposeClaim != "password_reset" || string.IsNullOrEmpty(emailClaim))
		{
			throw new InvalidOperationException("Invalid token type.");
		}

		// 3. Fetch ONLY the requested user directly from the database
		// (You may need to add this method to your repository if it doesn't exist)
		var user = await _uow.Users.GetByEmailAsync(emailClaim) ?? throw new InvalidOperationException("User not found.");

		// 4. Update the password
		user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

		// 5. Implicitly confirm their email
		if (!user.EmailConfirmed)
		{
			user.EmailConfirmed = true;
		}

		// 6. Save changes
		_uow.Users.Update(user);
		await _uow.SaveChangesAsync();
	}
}