using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scratchpad.Entities;

namespace Scratchpad
{
	public class CandleEntityTypeConfiguration : IEntityTypeConfiguration<CandleEntity>
	{
		public void Configure(EntityTypeBuilder<CandleEntity> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Open).HasColumnType("REAL");
			builder.Property(x => x.High).HasColumnType("REAL");
			builder.Property(x => x.Low).HasColumnType("REAL");
			builder.Property(x => x.Close).HasColumnType("REAL");

			builder.HasIndex(x => x.Symbol);
			builder.HasIndex(x => x.OpenDateTime);
			builder.HasIndex(x => x.CloseDateTime);
		}
	}
}