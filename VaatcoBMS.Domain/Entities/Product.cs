
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VaatcoBMS.Domain.Entities;

public class Product
{
	[Key]
	public int Id { get; set; }

	[Required]
	[MaxLength(30)]
	public string Code { get; set; } = string.Empty; // e.g. V104 — unique, enforced at DB/schema level

	[Required]
	[MaxLength(200)]
	public string Name { get; set; } = string.Empty;

	[Required]
	[MaxLength(50)]
	public string PackSize { get; set; } = string.Empty; // e.g. "100 ml"

	[Required]
	[Column(TypeName = "decimal(18,2)")]
	public decimal Price { get; set; }

	/// <summary>
	/// Current stock. Defaults to 0.
	/// </summary>
	public int StockQuantity { get; set; } = 0;

	/// <summary>
	/// Soft delete flag. Defaults to true.
	/// </summary>
	public bool IsActive { get; set; } = true;

	/// <summary>
	/// Row timestamp / created at. Populated when entity is created.
	/// </summary>
	[Required]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}