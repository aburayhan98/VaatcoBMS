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
}

