
namespace VaatcoBMS.Domain.Entities;

public class Customer
{
	public int Id { get; set; }

	// Business name
	public string Name { get; set; } = string.Empty;

	// Contact person (optional)
	public string? ContactPerson { get; set; }

	// Full address
	public string Address { get; set; } = string.Empty;

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
