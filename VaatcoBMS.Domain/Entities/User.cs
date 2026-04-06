
using VaatcoBMS.Domain.Enums;

namespace VaatcoBMS.Domain.Entities;

public class User : BaseEntity
{
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string PasswordHash { get; set; } = string.Empty;
	public UserRole Role { get; set; }
	public bool IsApproved { get; set; } = false;
	public bool EmailConfirmed { get; set; } = false;
	public string? RefreshToken { get; set; }
	public DateTime? RefreshTokenExpiry { get; set; }
	public string? EmailVerificationToken { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
