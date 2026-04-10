using AutoMapper;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using VaatcoBMS.Application;
using VaatcoBMS.Application.DTOs.User;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Application.Model.Auth;
using VaatcoBMS.Domain.Entities;
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

	public async Task<TokenResponse> LoginAsync(LoginModel model)
	{
		var users = await _uow.Users.GetAllAsync();
		var user = users.FirstOrDefault(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase));

		if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
		{
			_logger.LogWarning("Failed login attempt for email: {Email}", model.Email);
			throw new UnauthorizedAccessException("Invalid email or password.");
		}

		if (!user.IsApproved)
		{
			_logger.LogWarning("Login attempt for unapproved/inactive account: {Email}", model.Email);
			throw new UnauthorizedAccessException("Account is pending approval.");
		}

		_logger.LogInformation("Successful login for user: {Email}", user.Email);
		// Automatically returns both Access and Refresh token
		return _tokenBuilder.BuildTokens(user.Email, user.Id, user.Name, user.Role.ToString());
	}

	public TokenResponse RefreshLogin(string refreshToken)
	{
		var newTokens = _tokenBuilder.RefreshTokens(refreshToken);
		return newTokens ?? throw new UnauthorizedAccessException("Invalid refresh token.");
	}

	public async Task<UserDto> RegisterAsync(Register model)
	{
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

		// Generate Email Token and Send Email
		try 
		{
			var tokenString = _tokenBuilder.BuildEmailToken(user, "email_verification");
			var verificationLink = $"https://yourdomain.com/VerifyEmail?token={tokenString}";
			var emailBody = $"<p>Welcome {user.Name}!</p><p>Verify your email: <a href='{verificationLink}'>Click Here</a></p>";
			await _emailService.SendEmailAsync(user.Email, "Verify Your Account", emailBody);
		}
		catch(Exception ex)
		{
			_logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
		}

		return _mapper.Map<UserDto>(user);
	}

	public async Task<bool> VerifyEmailAsync(string token)
	{
		if (!_tokenBuilder.IsJwtValid(token)) return false;

		var claims = _tokenBuilder.GetClaims(token);
		var emailClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

		if (string.IsNullOrEmpty(emailClaim)) return false;

		var users = await _uow.Users.GetAllAsync();
		var user = users.FirstOrDefault(u => u.Email.Equals(emailClaim, StringComparison.OrdinalIgnoreCase));

		if (user == null) return false;

		user.IsApproved = true; 
		_uow.Users.Update(user);
		await _uow.SaveChangesAsync();

		return true;
	}

	public async Task ForgotPasswordAsync(string email)
	{
		var users = await _uow.Users.GetAllAsync();
		var user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

		// For security reasons, if the user isn't found, we silently return
		// so attackers can't verify which emails exist in the database.
		if (user == null)
		{
			_logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
			return;
		}

		try
		{
			// Generate a token explicitly marked for password resets
			var resetToken = _tokenBuilder.BuildEmailToken(user, "password_reset");
			
			// Replace with your actual frontend domain/URL
			var resetLink = $"https://yourdomain.com/ResetPassword?token={resetToken}";
			
			var emailBody = $"<p>Hi {user.Name},</p><p>You requested a password reset. Click the link below to set a new password:</p><p><a href='{resetLink}'>Reset Password</a></p>";
			
			await _emailService.SendEmailAsync(user.Email, "Reset Your Password", emailBody);
			_logger.LogInformation("Password reset email sent to {Email}", user.Email);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
		}
	}

	public async Task ResetPasswordAsync(ResetPasswordModel model)
	{
		// 1. Verify the token is valid and not expired
		if (!_tokenBuilder.IsJwtValid(model.Token))
		{
			throw new InvalidOperationException("Invalid or expired password reset token.");
		}

		// 2. Extract claims to ensure it's a reset token and get the email
		var claims = _tokenBuilder.GetClaims(model.Token);
		var purposeClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
		var emailClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

		if (purposeClaim != "password_reset" || string.IsNullOrEmpty(emailClaim))
		{
			throw new InvalidOperationException("Invalid token type.");
		}

		// 3. Find the user
		var users = await _uow.Users.GetAllAsync();
		var user = users.FirstOrDefault(u => u.Email.Equals(emailClaim, StringComparison.OrdinalIgnoreCase));

		if (user == null)
		{
			throw new InvalidOperationException("User associated with this token no longer exists.");
		}

		// 4. Update the password
		user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
		
		_uow.Users.Update(user);
		await _uow.SaveChangesAsync();
		
		_logger.LogInformation("Password successfully reset for {Email}", user.Email);
	}
}

