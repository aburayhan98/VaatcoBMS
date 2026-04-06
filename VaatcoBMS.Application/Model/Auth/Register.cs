using VaatcoBMS.Domain.Enums;

namespace VaatcoBMS.Application.Model.Auth;
//This is DTO class
public class Register
{
	public string Name { get; set; } = ""; 
	public string Email { get; set; } = ""; 
	public string Password { get; set; } = ""; 
	public UserRole Role { get; set; }
}
