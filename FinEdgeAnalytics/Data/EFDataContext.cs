using Microsoft.EntityFrameworkCore;

namespace FinEdgeAnalytics.Data
{
	public class EFDataContext : DbContext
	{
		public DbSet<EFData> IncomingData { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseInMemoryDatabase("InMemoryDb");
		}

		// Seeding the database
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<EFData>().HasKey(e => e.TransactionId);
		}

		public void SeedData()
		{
			if (IncomingData.Any()) return;

			IncomingData.AddRange(
				new EFData(Guid.NewGuid(), DateTime.Now.AddHours(-10), 19.99f, "Lorem Ipsum"),
				new EFData(Guid.NewGuid(), DateTime.Now.AddHours(-5), 50.00f, "Dolar Sitamet"),
				new EFData(Guid.NewGuid(), DateTime.Now.AddHours(-3), 71.20f, "Lorem Sitamet"),
				new EFData(Guid.NewGuid(), DateTime.Now.AddHours(-1), 35.50f, "Dolar Ipsum")
			);

			SaveChanges();
		}
	}
}
