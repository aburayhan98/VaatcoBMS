using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VaatcoBMS.Domain.Enums;

namespace VaatcoBMS.Domain.Entities;

public class Product : BaseEntity
{
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
	/// Threshold for low stock warning. Defaults to 5.
	/// </summary>
	public int LowStockThreshold { get; set; } = 5;

	private int _stockQuantity = 0;

	/// <summary>
	/// Current stock. Setting this automatically updates the StockStatus.
	/// </summary>
	public int StockQuantity 
	{ 
		get => _stockQuantity; 
		set 
		{
			_stockQuantity = value;
			UpdateStockStatus();
		}
	}

	/// <summary>
	/// Strongly typed domain status.
	/// </summary>
	public StockStatus StockStatus { get; private set; } = StockStatus.OutOfStock;

	/// <summary>
	/// Computed representation specifically for satisfying the UI / ProductDto string formatting.
	/// </summary>

	public bool IsActive { get; set; } = true;

	[Required]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public void Discontinue()
	{
		StockStatus = StockStatus.Discontinued;
	}

	private void UpdateStockStatus()
	{
		if (StockStatus == StockStatus.Discontinued) 
			return;

		if (_stockQuantity <= 0)
			StockStatus = StockStatus.OutOfStock;
		else if (_stockQuantity <= LowStockThreshold)
			StockStatus = StockStatus.LowStock;
		else
			StockStatus = StockStatus.InStock;
	}
}
