namespace VaatcoBMS.Application.DTOs.Product;

public class CreateProductDto
{
	public string ProductCode { get; set; } = ""; 
	public string ProductName { get; set; } = ""; 
	public string PackSize { get; set; } = ""; 
	public decimal Price { get; set; }
	public int StockQuantity { get; set; }
	
	// Add IsActive so the Create modal can set it
	public bool IsActive { get; set; } = true;
}
