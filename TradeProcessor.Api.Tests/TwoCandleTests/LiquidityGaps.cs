using FluentAssertions;
using TradeProcessor.Api.Domain;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Candles;

namespace TradeProcessor.Api.Tests.TwoCandleTests
{
	public class LiquidityGaps
	{
		[Fact]
		public void TwoBullishCandles()
		{
			var previous = new Candle(26510, 27105, 26395, 26395);
			var current = new Candle(27915, 28665, 27700, 28070);

			var threeCandles = new TwoCandles(previous, current);

			var expectedImbalance = new Imbalance(27700, 27105, BiasType.Bullish, GapType.Liquidity);

			threeCandles.TryFindImbalances(out var imbalance);

			imbalance.Should().Contain(expectedImbalance);
		}

		[Fact]
		public void TwoBearishCandles()
		{
			var previous = new Candle(28990, 27485, 27485, 27850);
			var current = new Candle(26475, 27005, 25600, 26020);

			var threeCandles = new TwoCandles(previous, current);

			var expectedImbalance = new Imbalance(27485, 27005, BiasType.Bearish, GapType.Liquidity);

			threeCandles.TryFindImbalances(out var imbalance);

			imbalance.Should().Contain(expectedImbalance);
		}
	}
}
