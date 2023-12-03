using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scratchpad.Entities;

namespace Scratchpad
{
	public class ImbalanceEntityTypeConfiguration : IEntityTypeConfiguration<ImbalanceEntity>
	{
		public void Configure(EntityTypeBuilder<OrderBlockEntity> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Open).HasColumnType("REAL");
			builder.Property(x => x.High).HasColumnType("REAL");
			builder.Property(x => x.Low).HasColumnType("REAL");
			builder.Property(x => x.Close).HasColumnType("REAL");

			builder.Property(x => x.Direction).HasConversion<string>();

			builder.HasIndex(x => x.Symbol);
			builder.HasIndex(x => x.OpenDateTime);
			builder.HasIndex(x => x.CloseDateTime);
		}

		public void Configure(EntityTypeBuilder<ImbalanceEntity> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.High).HasColumnType("REAL");
			builder.Property(x => x.Low).HasColumnType("REAL");

			builder.Property(x => x.BiasType).HasConversion<string>();
			builder.Property(x => x.GapType).HasConversion<string>();

			builder.HasIndex(x => x.Symbol);
		}
	}
}
