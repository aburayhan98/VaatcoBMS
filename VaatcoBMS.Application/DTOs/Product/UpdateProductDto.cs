namespace VaatcoBMS.Application.DTOs.Product;

public class UpdateProductDto
{
	public int Id { get; set; }
	public string ProductCode { get; set; } = ""; 
	public string ProductName { get; set; } = ""; 
	public string PackSize { get; set; } = ""; 
	public decimal Price { get; set; }
	public int StockQuantity { get; set; }
	
	// Add IsActive so the Edit modal can update it
	public bool IsActive { get; set; }
}
