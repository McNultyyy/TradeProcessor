using FluentAssertions;
using TradeProcessor.Api.Domain;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Candles;

namespace TradeProcessor.Api.Tests.TwoCandleTests
{
	public class OpeningGaps
	{
		[Fact]
		public void TwoBearishCandles()
		{
			var previous = new Candle(52, 52, 49.5, 50);
			var current = new Candle(50.25, 50.50, 48.50, 48.50);

			var threeCandles = new TwoCandles(previous, current);

			var expectedImbalance = new Imbalance(50.25m, 50, BiasType.Bearish, GapType.Opening);

			threeCandles.TryFindImbalances(out var imbalance);

			imbalance.Should().Contain(expectedImbalance);
		}

		[Fact]
		public void TwoBullishCandles()
		{
			var previous = new Candle(38.25, 39.75, 37.50, 39.75);
			var current = new Candle(39.50, 42, 39.50, 41.5);

			var threeCandles = new TwoCandles(previous, current);

			var expectedImbalance = new Imbalance(39.75m, 39.50m, BiasType.Bullish, GapType.Opening);

			threeCandles.TryFindImbalances(out var imbalance);

			imbalance.Should().Contain(expectedImbalance);
		}
	}
}
