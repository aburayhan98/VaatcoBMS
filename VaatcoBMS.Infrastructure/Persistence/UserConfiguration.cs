using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Infrastructure.Persistence;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> b)
	{
		b.HasKey(u => u.Id);

		b.HasIndex(u => u.Email)
				.IsUnique();

		b.Property(u => u.Name)
				.IsRequired()
				.HasMaxLength(100);

		b.Property(u => u.Email)
				.IsRequired()
				.HasMaxLength(200);

		b.Property(u => u.PasswordHash)
				.IsRequired(); // NVARCHAR(MAX) by default

		b.Property(u => u.Role)
				.IsRequired();

		b.Property(u => u.IsApproved)
				.HasDefaultValue(false);

		b.Property(u => u.EmailConfirmed)
				.HasDefaultValue(false);

		b.Property(u => u.CreatedAt)
				.HasColumnType("datetime2");
	}
}
