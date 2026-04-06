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
    /// User ID of the creator
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// Time of creation
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User ID of the last updater
    /// </summary>
    public int? UpdatedBy { get; set; }

    /// <summary>
    /// Time of last update
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
