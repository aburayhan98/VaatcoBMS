using System.ComponentModel.DataAnnotations;
using VaatcoBMS.Domain.Enums;

namespace VaatcoBMS.Application.Model.Auth;

// This is the DTO class for user registration
public class Register
{
	[Required(ErrorMessage = "Full Name is required.")]
	[StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
	public string Name { get; set; } = string.Empty;

	[Required(ErrorMessage = "Email Address is required.")]
	[EmailAddress(ErrorMessage = "Invalid Email Address format.")]
	public string Email { get; set; } = string.Empty;

	[Required(ErrorMessage = "Password is required.")]
	[StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
	// Require at least one lowercase, one uppercase, one digit and one non-alphanumeric (special) character.
	[RegularExpression(@"^(?=.{8,}$)(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).*$",
		ErrorMessage = "Password must have at least one uppercase letter, one lowercase letter, one number and one special character.")]
	public string Password { get; set; } = string.Empty;

	[Required(ErrorMessage = "Please select a System Role.")]
	public UserRole? Role { get; set; }
}
