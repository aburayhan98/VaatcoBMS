using System.ComponentModel.DataAnnotations;

namespace VaatcoBMS.Application.Model.Auth;

public class ForgotPasswordModel
{
	[Required]
	[EmailAddress]
	public string Email { get; set; }
}
