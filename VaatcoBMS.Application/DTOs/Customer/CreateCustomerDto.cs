

namespace VaatcoBMS.Application.DTOs.Customer;

public class CreateCustomerDto
{
	public string CustomerCode { get; set; } = ""; 
	public string Name { get; set; } = "";
	public string? ContactPerson { get; set; }
	public string Address { get; set; } = "";
	public string? City { get; set; }
	public string? District { get; set; }
	public string Phone { get; set; } = ""; 
	public string? Email { get; set; }
}
