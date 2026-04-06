namespace VaatcoBMS.Application;

public interface IBaseModel
{
	int Id { get; set; }
}

public abstract class BaseModel : IBaseModel
{
	public int Id { get; set; }
}
