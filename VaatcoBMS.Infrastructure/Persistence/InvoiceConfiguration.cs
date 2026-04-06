using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Infrastructure.Persistence;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
	public void Configure(EntityTypeBuilder<Invoice> b) 
	{ 
		b.HasKey(x => x.Id); b.HasIndex(x => x.InvoiceNumber).IsUnique(); 
		b.Property(x => x.InvoiceNumber).HasMaxLength(30).IsRequired(); 
		b.Property(x => x.Subtotal).HasPrecision(18, 2); 
		b.Property(x => x.Discount).HasPrecision(18, 2); 
		b.Property(x => x.VAT).HasPrecision(18, 2); 
		b.Property(x => x.TotalAmount).HasPrecision(18, 2); 
		b.HasOne(x => x.Customer).WithMany(c => c.Invoices).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict); 
		b.HasOne(i => i.CreatorUser)
       .WithMany()
       .HasForeignKey(i => i.CreatedBy) 
       .OnDelete(DeleteBehavior.Restrict); // Advisable for creator IDs
		b.HasMany(x => x.Items).WithOne(i => i.Invoice).HasForeignKey(i => i.InvoiceId).OnDelete(DeleteBehavior.Cascade); }
}
