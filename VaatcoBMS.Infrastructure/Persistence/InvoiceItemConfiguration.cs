using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Infrastructure.Persistence;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
	public void Configure(EntityTypeBuilder<InvoiceItem> builder)
	{
		builder.HasKey(i => i.Id);

		builder.Property(i => i.Quantity)
				.IsRequired();

		builder.Property(i => i.BonusQuantity)
				.HasDefaultValue(0);

		builder.Property(i => i.UnitPrice)
				.HasColumnType("decimal(18,2)")
				.IsRequired();

		// Total is a computed property (Quantity * UnitPrice), 
		// We tell EF Core to ignore it so it doesn't create a column in the DB
		builder.Ignore(i => i.Total);

		// Relationships
		builder.HasOne(i => i.Invoice)
				.WithMany(inv => inv.Items)
				.HasForeignKey(i => i.InvoiceId)
				.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(i => i.Product)
				.WithMany() // Assuming Product doesn't have a collection of InvoiceItems
				.HasForeignKey(i => i.ProductId)
				.OnDelete(DeleteBehavior.Restrict); // Prevent deleting products that are on invoices
	}
}
