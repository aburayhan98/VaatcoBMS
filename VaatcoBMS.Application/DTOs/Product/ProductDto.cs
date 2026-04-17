namespace VaatcoBMS.Application.DTOs.Product;

public class ProductDto
{
	public int Id { get; set; }
	public string ProductCode { get; set; } = ""; 
	public string ProductName { get; set; } = ""; 
	public string PackSize { get; set; } = ""; 
	public decimal Price { get; set; }
	public int StockQuantity { get; set; }
	public string StockStatus { get; set; } = ""; // "OK"|"Low"|"Out of Stock" 
	
	public bool IsActive { get; set; } 
}

