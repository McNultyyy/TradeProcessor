using FluentAssertions;
using TradeProcessor.Api.Domain;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Candles;

namespace TradeProcessor.Api.Tests.TwoCandleTests
{
	public class VolumeGaps
	{
		[Fact]
		public void TwoBearishCandles()
		{
			var previous = new Candle(64, 63, 61, 62);
			var current = new Candle(61, 61, 59, 59);

			var threeCandles = new TwoCandles(previous, current);

			var expectedImbalance = new Imbalance(62, 61, BiasType.Bearish, GapType.Volume);

			threeCandles.TryFindImbalances(out var imbalance);

			imbalance.Should().Contain(expectedImbalance);
		}

		[Fact]
		public void TwoBullishCandles()
		{
			var previous = new Candle(410, 414, 409, 412);
			var current = new Candle(414, 425, 411, 419);

			var threeCandles = new TwoCandles(previous, current);

			var expectedImbalance = new Imbalance(414, 412, BiasType.Bullish, GapType.Volume);

			threeCandles.TryFindImbalances(out var imbalance);

			imbalance.Should().Contain(expectedImbalance);
		}
	}
}
