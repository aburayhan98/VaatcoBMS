using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Infrastructure.Persistence;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
	public void Configure(EntityTypeBuilder<Customer> b)
	{
		b.HasKey(c => c.Id);

		b.Property(c => c.Name)
				.IsRequired()
				.HasMaxLength(200);

		b.Property(c => c.ContactPerson)
				.HasMaxLength(100);

		b.Property(c => c.Address)
				.IsRequired()
				.HasMaxLength(500);

		b.Property(c => c.Phone)
				.IsRequired()
				.HasMaxLength(30);

		b.Property(c => c.Email)
				.HasMaxLength(200);

		b.Property(c => c.IsActive)
				.HasDefaultValue(true);

		b.Property(c => c.CreatedAt)
				.HasColumnType("datetime2");
	}
}

