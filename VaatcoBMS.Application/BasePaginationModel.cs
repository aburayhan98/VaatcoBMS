namespace VaatcoBMS.Application;

public interface IBasePaginationModel
{
	int Total { get; set; }
}

public abstract class BasePaginationModel : IBasePaginationModel
{
	public int Total { get; set; }
}
