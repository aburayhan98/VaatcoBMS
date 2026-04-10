
using MapsterMapper;
using Microsoft.Extensions.Logging;
using VaatcoBMS.Application.DTOs.User;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Domain.Enums;
using VaatcoBMS.Domain.Interfaces;

namespace VaatcoBMS.Application.Services;

public class UserService : IUserService
{
	private readonly IUnitOfWork _uow;
	private readonly IMapper _mapper;
	private readonly IEmailService _email;
	private readonly ILogger<UserService> _logger;

	public UserService(IUnitOfWork uow, IMapper mapper, IEmailService email, ILogger<UserService> logger)
	{
		_uow = uow;
		_mapper = mapper;
		_email = email;
		_logger = logger;
	}

	// ── READ ────────────────────────────────────────────────

	public async Task<IEnumerable<UserDto>> GetAllAsync()
	{
		try
		{
			var users = await _uow.Users.GetAllAsync();
			var userCount = users.Count();

			_logger.LogDebug("Retrieved {Count} users from database", userCount);

			return _mapper.Map<IEnumerable<UserDto>>(users);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving all users");
			throw new ApplicationException("An error occurred while retrieving users", ex);
		}
	}

	public async Task<IEnumerable<UserDto>> GetPendingApprovalAsync()
	{
		try
		{
			var users = await _uow.Users.GetAllAsync();
			var pendingUsers = users.Where(u => !u.IsApproved && u.EmailConfirmed);
			var pendingCount = pendingUsers.Count();

			_logger.LogInformation("Retrieved {Count} pending approval users", pendingCount);

			return _mapper.Map<IEnumerable<UserDto>>(pendingUsers);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving pending approval users");
			throw new ApplicationException("An error occurred while retrieving pending approval users", ex);
		}
	}

	public async Task<UserDto?> GetByIdAsync(int id)
	{
		try
		{
			var user = await _uow.Users.GetByIdAsync(id);

			if (user == null)
			{
				_logger.LogDebug("User with Id: {UserId} not found", id);
				return null;
			}

			return _mapper.Map<UserDto>(user);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving user by Id: {UserId}", id);
			throw new ApplicationException($"An error occurred while retrieving user with Id: {id}", ex);
		}
	}

	public async Task<UserDto?> GetByEmailAsync(string email)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				_logger.LogWarning("GetByEmailAsync called with null or empty email");
				return null;
			}

			var users = await _uow.Users.GetAllAsync();
			var user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

			if (user == null)
			{
				_logger.LogDebug("User with email: {Email} not found", email);
				return null;
			}

			return _mapper.Map<UserDto>(user);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving user by email: {Email}", email);
			throw new ApplicationException($"An error occurred while retrieving user with email: {email}", ex);
		}
	}

	public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return false;
			}

			var users = await _uow.Users.GetAllAsync();

			var exists = users.Any(u =>
					u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
					(excludeId == null || u.Id != excludeId));

			if (exists)
			{
				_logger.LogDebug("Email '{Email}' already exists (ExcludeId: {ExcludeId})", email, excludeId);
			}

			return exists;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error checking if email exists: {Email}", email);
			throw new ApplicationException($"An error occurred while checking email: {email}", ex);
		}
	}

	// ── WRITE ───────────────────────────────────────────────

	public async Task<UserDto> UpdateProfileAsync(int id, UpdateProfileDto dto)
	{
		try
		{
			var user = await _uow.Users.GetByIdAsync(id);
			if (user == null)
			{
				_logger.LogWarning("User with Id: {UserId} not found for profile update", id);
				throw new KeyNotFoundException($"User {id} not found.");
			}

			// Check if email already exists (excluding current user)
			if (await EmailExistsAsync(dto.Email, id))
			{
				_logger.LogWarning("Attempted to update user {UserId} with existing email: {Email}", id, dto.Email);
				throw new InvalidOperationException("Email is already in use by another account.");
			}

			// Update user properties
			user.Name = dto.Name;
			user.Email = dto.Email;

			_uow.Users.Update(user);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("User profile updated successfully. UserId: {UserId}, Email: {Email}", id, user.Email);

			return _mapper.Map<UserDto>(user);
		}
		catch (KeyNotFoundException)
		{
			throw;
		}
		catch (InvalidOperationException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating profile for user Id: {UserId}", id);
			throw new ApplicationException($"An error occurred while updating profile for user Id: {id}", ex);
		}
	}

	public async Task ChangeRoleAsync(int id, UserRole newRole)
	{
		try
		{
			var user = await _uow.Users.GetByIdAsync(id);
			if (user == null)
			{
				_logger.LogWarning("User with Id: {UserId} not found for role change", id);
				throw new KeyNotFoundException($"User {id} not found.");
			}

			var oldRole = user.Role;
			user.Role = newRole;

			_uow.Users.Update(user);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("User role changed successfully. UserId: {UserId}, OldRole: {OldRole}, NewRole: {NewRole}",
					id, oldRole, newRole);
		}
		catch (KeyNotFoundException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error changing role for user Id: {UserId} to role: {NewRole}", id, newRole);
			throw new ApplicationException($"An error occurred while changing role for user Id: {id}", ex);
		}
	}

	public async Task ApproveUserAsync(int id)
	{
		try
		{
			var user = await _uow.Users.GetByIdAsync(id);
			if (user == null)
			{
				_logger.LogWarning("User with Id: {UserId} not found for approval", id);
				throw new KeyNotFoundException($"User {id} not found.");
			}

			if (user.IsApproved)
			{
				_logger.LogInformation("User {UserId} is already approved", id);
				return;
			}

			user.IsApproved = true;

			_uow.Users.Update(user);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("User approved successfully. UserId: {UserId}, Email: {Email}", id, user.Email);

			// Send approval email
			try
			{
				var subject = "Your VaatcoIMS account has been approved";
				var body = $@"
                    <html>
                    <body>
                        <h2>Welcome, {user.Name}!</h2>
                        <p>Your account has been approved. You can now log in.</p>
                        <br/>
                        <p>Best regards,<br/>VaatcoIMS Team</p>
                    </body>
                    </html>";

				await _email.SendEmailAsync(user.Email, subject, body);
				_logger.LogInformation("Approval email sent to {Email}", user.Email);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send approval email to {Email}", user.Email);
				// Don't throw - user is already approved, just log the error
			}
		}
		catch (KeyNotFoundException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error approving user Id: {UserId}", id);
			throw new ApplicationException($"An error occurred while approving user Id: {id}", ex);
		}
	}

	public async Task RejectUserAsync(int id)
	{
		try
		{
			var user = await _uow.Users.GetByIdAsync(id);
			if (user == null)
			{
				_logger.LogWarning("User with Id: {UserId} not found for rejection", id);
				throw new KeyNotFoundException($"User {id} not found.");
			}

			var userEmail = user.Email;
			var userName = user.Name;

			// Send rejection email
			try
			{
				var subject = "VaatcoIMS — Account Registration Update";
				var body = $@"
                    <html>
                    <body>
                        <p>Dear {userName},</p>
                        <p>Unfortunately your registration could not be approved. Please contact your administrator.</p>
                        <br/>
                        <p>Best regards,<br/>VaatcoIMS Team</p>
                    </body>
                    </html>";

				await _email.SendEmailAsync(userEmail, subject, body);
				_logger.LogInformation("Rejection email sent to {Email}", userEmail);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send rejection email to {Email}", userEmail);
				// Continue with deletion even if email fails
			}

			// Hard delete the user on rejection
			_uow.Users.Delete(user);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("User rejected and deleted successfully. UserId: {UserId}, Email: {Email}", id, userEmail);
		}
		catch (KeyNotFoundException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error rejecting user Id: {UserId}", id);
			throw new ApplicationException($"An error occurred while rejecting user Id: {id}", ex);
		}
	}

	public async Task ChangePasswordAsync(int id, ChangePasswordDto dto)
	{
		try
		{
			var user = await _uow.Users.GetByIdAsync(id);
			if (user == null)
			{
				_logger.LogWarning("User with Id: {UserId} not found for password change", id);
				throw new KeyNotFoundException($"User {id} not found.");
			}

			// Verify current password using BCrypt.Net.BCrypt
			if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
			{
				_logger.LogWarning("Invalid current password attempt for user Id: {UserId}", id);
				throw new UnauthorizedAccessException("Current password is incorrect.");
			}

			// Hash and set new password using BCrypt.Net.BCrypt
			user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

			_uow.Users.Update(user);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("Password changed successfully for user Id: {UserId}", id);
		}
		catch (KeyNotFoundException)
		{
			throw;
		}
		catch (UnauthorizedAccessException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error changing password for user Id: {UserId}", id);
			throw new ApplicationException($"An error occurred while changing password for user Id: {id}", ex);
		}
	}

	public async Task DeactivateAsync(int id)
	{
		try
		{
			var user = await _uow.Users.GetByIdAsync(id);
			if (user == null)
			{
				_logger.LogWarning("User with Id: {UserId} not found for deactivation", id);
				throw new KeyNotFoundException($"User {id} not found.");
			}

			if (!user.IsApproved)
			{
				_logger.LogInformation("User {UserId} is already deactivated", id);
				return;
			}

			// Revokes login access without deleting (soft deactivation)
			user.IsApproved = false;

			_uow.Users.Update(user);
			await _uow.SaveChangesAsync();

			_logger.LogInformation("User deactivated successfully. UserId: {UserId}, Email: {Email}", id, user.Email);
		}
		catch (KeyNotFoundException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deactivating user Id: {UserId}", id);
			throw new ApplicationException($"An error occurred while deactivating user Id: {id}", ex);
		}
	}
}