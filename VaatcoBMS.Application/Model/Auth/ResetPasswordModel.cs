using System.ComponentModel.DataAnnotations;

namespace VaatcoBMS.Application.Model.Auth;

public class ResetPasswordModel
{
	/// <summary>
	/// 
	/// </summary>
	[Required]
	[EmailAddress]
	public string Email { get; set; }

	/// <summary>
	/// 
	/// </summary>
	[Required]
	[MinLength(6)]
	public string Password { get; set; }

	/// <summary>
	/// 
	/// </summary>
	[Required]
	[MinLength(6)]
	[Compare("Password", ErrorMessage = "Passwords do not match.")]
	public string ConfirmPassword { get; set; }

	[Required]
	public string Token { get; set; }

}
