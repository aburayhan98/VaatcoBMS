namespace VaatcoBMS.Domain.Common;

public class CustomerQueryParams
{
  private const int MaxPageSize = 100;
  private int _pageSize = 20;

  public int Page { get; set; } = 1;

  public int PageSize
  {
    get => _pageSize;
    set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
  }

  public string? Search { get; set; }  // matches Name, CustomerCode, Phone, or ContactPerson
  public string? City { get; set; }    // filter by specific city
  public string? District { get; set; } // filter by specific district
  public bool? IsActive { get; set; }

  public string SortBy { get; set; } = "name"; // name | code | city | createdAt
  public string SortDir { get; set; } = "asc"; // asc | desc
}