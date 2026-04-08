using AutoMapper;
using Microsoft.Extensions.Logging;
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
			ILogger<AuthService> logger) : IAuthService
	{
		private readonly IUnitOfWork _uow = uow;
		private readonly IMapper _mapper = mapper;
		private readonly ITokenBuilder _tokenBuilder = tokenBuilder;
		private readonly ILogger<AuthService> _logger = logger;

		public async Task<string> LoginAsync(LoginModel model)
		{
			var users = await _uow.Users.GetAllAsync();
			var user = users.FirstOrDefault(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase));

			// Note: Using BCrypt.Net.BCrypt from your UserService context
			if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
			{
				_logger.LogWarning("Failed login attempt for email: {Email}", model.Email);
				throw new UnauthorizedAccessException("Invalid email or password.");
			}

			if (!user.IsApproved)
			{
				_logger.LogWarning("Login attempt for unapproved/inactive account: {Email}", model.Email);
				throw new UnauthorizedAccessException("Account is not active or pending approval.");
			}

			_logger.LogInformation("Successful login for user: {Email}", user.Email);

			// Ensure your TokenBuilder is generating the JWT string appropriately
			return _tokenBuilder.BuildToken(user.Email, user.Id, user.Role.ToString());
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
			user.IsApproved = false; // Default to needing approval
			user.CreatedAt = DateTime.UtcNow;

			await _uow.Users.AddAsync(user);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("New user registered successfully: {Email}", user.Email);

			return _mapper.Map<UserDto>(user);
		}
	}

