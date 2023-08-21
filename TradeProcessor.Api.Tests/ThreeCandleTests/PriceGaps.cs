using FluentAssertions;
using TradeProcessor.Api.Domain;
using TradeProcessor.Api.Domain.Candles;

namespace TradeProcessor.Api.Tests.ThreeCandleTests
{
	public class ThreeCandleTests
	{
		[Fact]
		public void ThreeBearishCandlesShouldGiveABearishImbalance()
		{
			var previousPrevious = new Candle(30, 31, 28, 29);
			var previous = new Candle(29, 29, 25, 25);
			var current = new Candle(25, 26, 20, 21);

			var threeCandles = new ThreeCandles(previousPrevious, previous, current);

			var expectedImbalance = new Imbalance(28, 26, BiasType.Bearish, GapType.Price);

			threeCandles.TryFindImbalance(out var imbalance);

			imbalance.Should().Be(expectedImbalance);
		}

		[Fact]
		public void OneBullishCandleAndTwoBearishCandlesShouldGiveABearishImbalance()
		{
			var previousPrevious = new Candle(30075, 30078, 30057, 30063);
			var previous = new Candle(30075, 30075, 29990, 30005);
			var current = new Candle(30005, 30013, 29952, 29985);

			var threeCandles = new ThreeCandles(previousPrevious, previous, current);

			var expectedImbalance = new Imbalance(30057, 30013, BiasType.Bearish, GapType.Price);

			threeCandles.TryFindImbalance(out var imbalance);

			imbalance.Should().Be(expectedImbalance);
		}

		[Fact]
		public void TwoBearishCandlesAndOneBullishCandleShouldGiveABearishImbalance()
		{
			var previousPrevious = new Candle(29843, 29849, 29814, 29821);
			var previous = new Candle(29821, 29825, 29733, 29748);
			var current = new Candle(29748, 29786, 29700, 29784);

			var threeCandles = new ThreeCandles(previousPrevious, previous, current);

			var expectedImbalance = new Imbalance(29814, 29786, BiasType.Bearish, GapType.Price);

			threeCandles.TryFindImbalance(out var imbalance);

			imbalance.Should().Be(expectedImbalance);
		}

		[Fact]
		public void AdHocTest()
		{
			var previousPrevious = new Candle(1.733, 1.733, 1.732, 1.733);
			var previous = new Candle(1.733, 1.736, 1.731, 1.736);
			var current = new Candle(1.736, 1.737, 1.736, 1.736);

			var fvg = new ThreeCandles(previousPrevious, previous, current);

			var expectedImbalance = new Imbalance(1.736m, 1.733m, BiasType.Bullish, GapType.Price);

			fvg.TryFindImbalance(out var imbalance);

			imbalance.Should().Be(expectedImbalance);

		}
	}
}
