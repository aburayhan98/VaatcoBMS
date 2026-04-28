namespace VaatcoBMS.Domain.Common;

public class PagedResult<T>
{
	public IEnumerable<T> Items { get; init; } = [];
	public int TotalCount { get; init; }
	public int Page { get; init; }
	public int PageSize { get; init; }
	public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
	public bool HasPrev => Page > 1;
	public bool HasNext => Page < TotalPages;
}
