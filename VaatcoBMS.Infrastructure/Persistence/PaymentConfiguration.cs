using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Infrastructure.Persistence;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
  public void Configure(EntityTypeBuilder<Payment> builder)
  {
    builder.HasKey(p => p.Id);

    // Resolve the precision warning for Amount
    builder.Property(p => p.Amount)
      .IsRequired()
      .HasPrecision(18, 2);

    builder.Property(p => p.Method)
      .IsRequired()
      .HasMaxLength(50);

    builder.Property(p => p.Reference)
      .HasMaxLength(100);

    builder.Property(p => p.Notes)
      .HasMaxLength(500);

    // Foreign Key relationship to Invoice
    builder.HasOne(p => p.Invoice)
      .WithMany(i => i.Payments)  // <- explicitly map it to the Payments collection on Invoice
      .HasForeignKey(p => p.InvoiceId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}