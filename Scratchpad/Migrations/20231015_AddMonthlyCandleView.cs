using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scratchpad.Migrations
{
	/// <inheritdoc />
	public partial class AddMonthlyCandleView : Migration
	{
		private const string ViewName = "MonthlyCandles";

		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@$"
create view {ViewName} as
select 
	symbol,	
	min(Low) as Low,
	max(High) as High,
	strftime('%Y-%m-01', OpenDateTime) as DateTime
from Candles
group by
	symbol,
	strftime('%m', OpenDateTime),
	strftime('%Y', OpenDateTime)
");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@$"
drop view {ViewName}
");
		}
	}
}
