using System.ComponentModel.DataAnnotations;

namespace VaatcoBMS.Application.Model.Auth;

public class LoginModel
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
}
