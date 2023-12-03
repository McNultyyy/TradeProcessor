using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scratchpad.Entities;

namespace Scratchpad;

public class PricesContext : DbContext
{
	public DbSet<CandleEntity> Candles { get; set; }
	public DbSet<OrderBlockEntity> OrderBlocks { get; set; }
	public DbSet<ImbalanceEntity> Imbalances { get; set; }

	public string DbPath { get; }

	public PricesContext()
	{
		var path = "C:\\Users\\willi\\Projects\\TradeProcessor";
		DbPath = System.IO.Path.Join(path, "prices.db");
	}

	// The following configures EF to create a Sqlite database file in the
	// special "local" folder for your platform.
	protected override void OnConfiguring(DbContextOptionsBuilder options)
		=> options
			.UseSqlite($"Data Source={DbPath}")
			.EnableSensitiveDataLogging();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		new CandleEntityTypeConfiguration().Configure(modelBuilder.Entity<CandleEntity>());
		new OrderBlockEntityTypeConfiguration().Configure(modelBuilder.Entity<OrderBlockEntity>());
		new ImbalanceEntityTypeConfiguration().Configure(modelBuilder.Entity<ImbalanceEntity>());
	}
}
