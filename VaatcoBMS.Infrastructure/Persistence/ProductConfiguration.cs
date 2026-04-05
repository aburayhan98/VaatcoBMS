using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Infrastructure.Persistence;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
		public void Configure(EntityTypeBuilder<Product> b)
	  {
			// Primary Key
			b.HasKey(p => p.Id);

			// Unique constraint on Code
			b.HasIndex(p => p.Code).IsUnique();
			b.Property(p => p.Code)
			 .HasMaxLength(30)
			 .IsRequired();

			// Properties
			b.Property(p => p.Name)
			 .HasMaxLength(200)
			 .IsRequired();

			b.Property(p => p.PackSize)
			 .HasMaxLength(50)
			 .IsRequired();

			b.Property(p => p.Price)
			 .HasColumnType("decimal(18,2)")
			 .IsRequired();

			// Default values based on the initial table schema
			b.Property(p => p.StockQuantity)
			 .HasDefaultValue(0);

			b.Property(p => p.IsActive)
			 .HasDefaultValue(true);

			b.Property(p => p.CreatedAt)
			 .HasColumnType("datetime2")
			 .IsRequired();
	}
}

