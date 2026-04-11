using MapsterMapper;
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
		ILogger<AuthService> logger) : IAuthService
{
	private readonly IUnitOfWork _uow = uow;
	private readonly IMapper _mapper = mapper;
	private readonly ITokenBuilder _tokenBuilder = tokenBuilder;
	private readonly IEmailService _emailService = emailService;
	private readonly ILogger<AuthService> _logger = logger;

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
	public async Task<UserDto> RegisterAsync(Register model)
	{
		// Block self-registration as Admin or SuperAdmin
		if (model.Role == UserRole.Admin || model.Role == UserRole.SuperAdmin)
		{
			throw new InvalidOperationException("Admin and SuperAdmin accounts cannot be self-registered.");
		}

		var users = await _uow.Users.GetAllAsync();

		if (users.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
		{
			throw new InvalidOperationException("Email is already registered.");
		}

		var user = _mapper.Map<User>(model);
		user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
		user.IsApproved = false;
		user.CreatedAt = DateTime.UtcNow;

		await _uow.Users.AddAsync(user);
		await _uow.SaveChangesAsync();

		try
		{
			var token = _tokenBuilder.BuildEmailToken(user, "email_verification");
			var link = $"https://yourdomain.com/auth/verify-email?token={token}";
			var body = $"<p>Hi {user.Name},</p>" +
								 $"<p>Verify your email: <a href='{link}'>Click Here</a></p>";

			await _emailService.SendEmailAsync(user.Email, "Verify Your Account", body);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
		}

		return _mapper.Map<UserDto>(user);
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
			var link = $"https://yourdomain.com/auth/reset-password?token={token}";
			var body = $"<p>Hi {user.Name},</p>" +
								 $"<p>Reset your password: <a href='{link}'>Click Here</a></p>";

			await _emailService.SendEmailAsync(user.Email, "Reset Your Password", body);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Password reset email failed for {Email}", email);
		}
	}

	// ── RESET PASSWORD ───────────────────────────────────────────────
	public async Task ResetPasswordAsync(ResetPasswordModel model)
	{
		if (!_tokenBuilder.IsJwtValid(model.Token))
		{
			throw new InvalidOperationException("Invalid or expired password reset token.");
		}

		var claims = _tokenBuilder.GetClaims(model.Token);
		var purposeClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
		var emailClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

		if (purposeClaim != "password_reset" || string.IsNullOrEmpty(emailClaim))
		{
			throw new InvalidOperationException("Invalid token type.");
		}

		var users = await _uow.Users.GetAllAsync();
		var user = users.FirstOrDefault(u => u.Email.Equals(emailClaim, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException("User not found.");
		user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
		_uow.Users.Update(user);
		await _uow.SaveChangesAsync();
	}
}