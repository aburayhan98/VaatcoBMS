using Microsoft.EntityFrameworkCore;
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}
	// DbSet properties for your entities, e.g.:
	 public DbSet<Customer> Customers { get; set; }
	 public DbSet<Product> Products { get; set; }
	 public DbSet<Invoice> Invoices { get; set; }
	 public DbSet<InvoiceItem> InvoiceItems { get; set; }
	public DbSet<User> Users { get; set; }
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
		// Configure entity relationships, constraints, etc. here
	}
}
