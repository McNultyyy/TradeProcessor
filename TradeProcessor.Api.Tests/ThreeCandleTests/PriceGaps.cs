using FluentAssertions;
using FluentAssertions.Execution;
using TradeProcessor.Api.Domain;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Candles;

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

			threeCandles.TryFindImbalances(out var imbalance);

			imbalance.Should().Contain(expectedImbalance);
		}

		[Fact]
		public void OneBullishCandleAndTwoBearishCandlesShouldGiveABearishImbalance()
		{
			var previousPrevious = new Candle(30075, 30078, 30057, 30063);
			var previous = new Candle(30075, 30075, 29990, 30005);
			var current = new Candle(30005, 30013, 29952, 29985);

			var threeCandles = new ThreeCandles(previousPrevious, previous, current);

			var expectedImbalance = new Imbalance(30057, 30013, BiasType.Bearish, GapType.Price);

			threeCandles.TryFindImbalances(out var imbalance);

			imbalance.Should().Contain(expectedImbalance);
		}

		[Fact]
		public void TwoBearishCandlesAndOneBullishCandleShouldGiveABearishImbalance()
		{
			var previousPrevious = new Candle(29843, 29849, 29814, 29821);
			var previous = new Candle(29821, 29825, 29733, 29748);
			var current = new Candle(29748, 29786, 29700, 29784);

			var threeCandles = new ThreeCandles(previousPrevious, previous, current);

			var expectedImbalance = new Imbalance(29814, 29786, BiasType.Bearish, GapType.Price);

			threeCandles.TryFindImbalances(out var imbalance);

			imbalance.Should().Contain(expectedImbalance);
		}

		[Fact]
		public void AdHocTest()
		{
			var previousPrevious = new Candle(1.733, 1.733, 1.732, 1.733);
			var previous = new Candle(1.733, 1.736, 1.731, 1.736);
			var current = new Candle(1.736, 1.737, 1.736, 1.736);

			var fvg = new ThreeCandles(previousPrevious, previous, current);

			var expectedImbalance = new Imbalance(1.736m, 1.733m, BiasType.Bullish, GapType.Price);

			fvg.TryFindImbalances(out var imbalance);

			imbalance.Should().Contain(expectedImbalance);

		}

		[Fact]
		public void MultipleGapTest()
		{
			var previousPrevious = new Candle(27840, 27955, 27810, 27855);
			var previous = new Candle(27855, 27900, 27485, 27620);
			var current = new Candle(26475, 26925, 25925, 26850);

			var fvg = new ThreeCandles(previousPrevious, previous, current);


			fvg.TryFindImbalances(out var imbalances);


			using (new AssertionScope())
			{
				imbalances.Should().Match(x => x.Any(y => y.GapType == GapType.Liquidity));
				imbalances.Should().Match(x => x.Any(y => y.GapType == GapType.Volume));
				imbalances.Should().Match(x => x.Any(y => y.GapType == GapType.Price));
			}
		}
	}
}
