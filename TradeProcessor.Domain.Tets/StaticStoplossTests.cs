using FluentAssertions;
using TradeProcessor.Domain.Stoploss;

namespace TradeProcessor.Domain.Tets
{
	public class StaticStoplossTests
	{
		[Fact]
		public void Test1()
		{
			var stoploss = 50m;

			var staticStoploss = new StaticStoploss(stoploss);

			staticStoploss.Result().Should().Be(stoploss);
		}
	}

	public class PercentageStoplossTests
	{
		[Fact]
		public void Test1()
		{
			var percentage = 1m;
			var entryPrice = 100m;
			var isBullish = true;

			var percentageStoploss = new PercentageStoploss(percentage, entryPrice, isBullish);

			var expected = 99m;

			percentageStoploss.Result().Should().Be(expected);
		}

		[Fact]
		public void Test2()
		{
			var percentage = 1m;
			var entryPrice = 100m;
			var isBullish = false;

			var percentageStoploss = new PercentageStoploss(percentage, entryPrice, isBullish);

			var expected = 101m;

			percentageStoploss.Result().Should().Be(expected);
		}
	}

	public class RelativeStoplossTests
	{
		[Fact]
		public void Test1()
		{
			var offset = 5m;
			var entryPrice = 100m;
			var isBullish = true;

			var percentageStoploss = new RelativeStoploss(entryPrice, offset, isBullish);

			var expected = 95m;

			percentageStoploss.Result().Should().Be(expected);
		}

		[Fact]
		public void Test2()
		{
			var offset = 5m;
			var entryPrice = 100m;
			var isBullish = false;

			var percentageStoploss = new RelativeStoploss(entryPrice, offset, isBullish);

			var expected = 105m;

			percentageStoploss.Result().Should().Be(expected);
		}
	}


}
