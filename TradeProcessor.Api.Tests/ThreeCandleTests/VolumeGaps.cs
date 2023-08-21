using FluentAssertions;
using TradeProcessor.Api.Domain;
using TradeProcessor.Api.Domain.Candles;

namespace TradeProcessor.Api.Tests.ThreeCandleTests
{
	public class VolumeGaps
	{
		[Fact]
		public void ThreeBearishCandlesWithBottomVIB()
		{
			var previousPrevious = new Candle(65, 665, 62, 63);
			var previous = new Candle(64, 63, 61, 62);
			var current = new Candle(61, 61, 59, 59);

			var threeCandles = new ThreeCandles(previousPrevious, previous, current);

			var expectedImbalance = new Imbalance(62, 61, BiasType.Bearish, GapType.Volume);

			threeCandles.TryFindImbalance(out var imbalance);

			imbalance.Should().Be(expectedImbalance);
		}

		[Fact]
		public void ThreeBearishCandlesWithTopVIB()
		{
			var previousPrevious = new Candle(404, 404, 394, 396);
			var previous = new Candle(394, 394, 382, 385);
			var current = new Candle(385, 389, 375, 375);

			var threeCandles = new ThreeCandles(previousPrevious, previous, current);

			var expectedImbalance = new Imbalance(396, 394, BiasType.Bearish, GapType.Volume);

			threeCandles.TryFindImbalance(out var imbalance);

			imbalance.Should().Be(expectedImbalance);
		}

		[Fact]
		public void ThreeBullishCandlesWithTopVIB()
		{
			var previousPrevious = new Candle(405, 411, 404, 410);
			var previous = new Candle(410, 414, 409, 412);
			var current = new Candle(414,425,411,419);

			var threeCandles = new ThreeCandles(previousPrevious, previous, current);

			var expectedImbalance = new Imbalance(414, 412, BiasType.Bullish, GapType.Volume);

			threeCandles.TryFindImbalance(out var imbalance);

			imbalance.Should().Be(expectedImbalance);
		}

		[Fact]
		public void ThreeBullishCandlesWithBottomVIB()
		{
			var previousPrevious = new Candle(410, 414, 409, 412);
			var previous = new Candle(414,425,411,419);
			var current = new Candle(419, 419, 415, 417);

			var threeCandles = new ThreeCandles(previousPrevious, previous, current);

			var expectedImbalance = new Imbalance(414, 412, BiasType.Bullish, GapType.Volume);

			threeCandles.TryFindImbalance(out var imbalance);

			imbalance.Should().Be(expectedImbalance);
		}
	}
}
