using System.Globalization;
using TradeProcessor.Domain;

namespace OrderBlocks.Tests
{
	public class UnitTest1
	{
		[Fact]
		public async Task Test1()
		{
			/*
			 * Given that the liquidity is the first pivot in the date range
			 * Then a valid order block should be found
			 */

			var symbol = "MATICUSDT";
			var startDate = DateTime.ParseExact("2023-08-20", "yyyy-MM-dd", new DateTimeFormatInfo());
			var endDate = DateTime.ParseExact("2023-09-10", "yyyy-MM-dd", new DateTimeFormatInfo());
			var interval = TimeSpan.FromHours(24);
			var bias = BiasType.Bearish;

			var orderBlock = await new Do().Work(symbol, startDate, endDate, interval, bias);


		}


		[Fact]
		public async Task Test2()
		{
			var symbol = "BCHUSDT";

			var interval = TimeSpan.FromHours(24);
			var endDate = DateTime.ParseExact("2023-08-29", "yyyy-MM-dd", CultureInfo.InvariantCulture);
			var startDate = endDate - (10 * interval);

			var bias = BiasType.Bearish;

			var orderBlock = await new Do().Work(symbol, startDate, endDate, interval, bias);
		}

		[Fact]
		public async Task Test3()
		{
			var symbol = "JOEUSDT";

			var interval = TimeSpan.FromHours(4);
			var endDate = DateTime.ParseExact("2023-09-09", "yyyy-MM-dd", CultureInfo.InvariantCulture);
			var startDate = endDate - (10 * interval);

			var bias = BiasType.Bearish;

			var orderBlock = await new Do().Work(symbol, startDate, endDate, interval, bias);
		}
	}
}
