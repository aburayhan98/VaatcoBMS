
namespace VaatcoBMS.Domain.Entities;

public class Customer : BaseEntity
{
	
	// Business name
	public string Name { get; set; } = string.Empty;
	// Unique customer code (optional)
	public string? CustomerCode { get; set; }

	// Contact person (optional)
	public string? ContactPerson { get; set; }

	// Full address
	public string Address { get; set; } = string.Empty;

	// City
	public string? City { get; set; } = string.Empty;

	//District
	public string? District { get; set; }


	// Contact number
	public string Phone { get; set; } = string.Empty;

	// Email address (optional)
	public string? Email { get; set; }

	// Row timestamp / created at
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	// Soft delete flag
	public bool IsActive { get; set; } = true;

	// Navigation property
	public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
