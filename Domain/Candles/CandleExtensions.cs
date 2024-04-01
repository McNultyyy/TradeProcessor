using Skender.Stock.Indicators;

namespace TradeProcessor.Domain.Candles
{
	public static class CandleExtensions
	{
		public static IQuote ToQuote(this ICandle candle)
		{
			return new Quote
			{
				Open = candle.Open,
				High = candle.High,
				Low = candle.Low,
				Close = candle.Close,
				Date = candle.OpenDateTime
			};
		}

		public static TimeSpan GetInterval(this ICandle candle)
		{
			return candle.CloseDateTime - candle.OpenDateTime;
		}

		public static bool IsInSameDirection(this ICandle candle, ICandle otherCandle)
		{
			return candle.IsBullishCandle() == otherCandle.IsBullishCandle();
		}

		public static bool IsBullishCandle(this ICandle candle)
		{
			return candle.Close > candle.Open;
		}

		public static bool IsBearishCandle(this ICandle candle)
		{
			return candle.Close < candle.Open;
		}

		public static bool ClosesBelowOpen(this ICandle candle, ICandle otherCandle)
		{
			return candle.Close < otherCandle.Open;
		}


		public static bool ClosesAboveOpen(this ICandle candle, ICandle otherCandle)
		{
			return candle.Close > otherCandle.Open;
		}

		public static bool HasHigherHighThan(this ICandle candle, ICandle otherCandle)
		{
			return candle.High > otherCandle.High;
		}


		public static bool IsAfter(this ICandle candle, ICandle otherCandle)
		{
			return candle.OpenDateTime > otherCandle.OpenDateTime;
		}

		public static bool HasLowerLowThan(this ICandle candle, ICandle otherCandle)
		{
			return candle.Low < otherCandle.Low;
		}
	}
}
