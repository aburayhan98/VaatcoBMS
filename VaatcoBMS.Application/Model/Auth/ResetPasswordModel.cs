using System.ComponentModel.DataAnnotations;

namespace VaatcoBMS.Application.Model.Auth;

public class ResetPasswordModel
{
	[EmailAddress]
	public string? Email { get; set; }

	[Required]
	[MinLength(6)]
	public string? Password { get; set; }

	[Required]
	[MinLength(6)]
	[Compare("Password", ErrorMessage = "Passwords do not match.")]
	public string? ConfirmPassword { get; set; }

	[Required]
	public string? ResetToken { get; set; } // Rename this from Token to ResetToken
}
