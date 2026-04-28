namespace VaatcoBMS.Domain.Common;

public class ProductQueryParams
{
	private const int MaxPageSize = 100;
	private int _pageSize = 20;

	public int Page { get; set; } = 1;

	public int PageSize
	{
		get => _pageSize;
		set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
	}

	public string? Search { get; set; }        // matches Name or Code
	public string? StockStatus { get; set; }   // "OK" | "Low" | "Out of Stock"
	public bool? IsActive { get; set; }

	public string SortBy { get; set; } = "name";   // name | code | price | stock
	public string SortDir { get; set; } = "asc";    // asc  | desc
}
