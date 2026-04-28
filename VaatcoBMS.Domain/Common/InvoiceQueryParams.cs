namespace VaatcoBMS.Domain.Common;

public class InvoiceQueryParams
{
  private const int MaxPageSize = 100;
  private int _pageSize = 20;

  public int Page { get; set; } = 1;

  public int PageSize
  {
    get => _pageSize;
    set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
  }

  public string? Search { get; set; }        // matches InvoiceNumber or ReferenceNumber
  public int? CustomerId { get; set; }       // filter by specific customer
  public string? Status { get; set; }        // filter by InvoiceStatus (e.g. "Draft", "Approved", "Paid")

  public DateTime? StartDate { get; set; }   // filter invoices starting from dates
  public DateTime? EndDate { get; set; }     // filter invoices up to dates

  public string SortBy { get; set; } = "date";   // date | number | total | status
  public string SortDir { get; set; } = "desc";  // asc  | desc (default desc to show newest invoices first)
}