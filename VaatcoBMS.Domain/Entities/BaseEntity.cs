namespace VaatcoBMS.Domain.Entities;

public class BaseEntity
{
	/// <summary>
	/// Id of the entity
	/// </summary>
	public virtual int Id { get; set; }
}

public class BaseEntityWithAudit : BaseEntity
{
	/// <summary>
	/// 
	/// </summary>
	public int UpdatedBy { get; set; }

	/// <summary>
	/// 
	/// </summary>
	public DateTime UpdatedAt { get; set; }
}
