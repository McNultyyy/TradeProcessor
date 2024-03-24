using ListGenerator.ListGenerators;
using TradeProcessor.Domain;

namespace ListGenerator
{
	public enum Direction
	{
		Bullish,
		Bearish
	}
	
	public static class Extensions
	{
		public static string SymbolToTradingViewSymbol(this Symbol symbol)
		{
			return $"{symbol.Exchange}:{symbol}{(symbol.SymbolType == SymbolType.Perpetual ? ".P" : "")}";
		}

		public static (TOut first, TOut second, TOut third, TOut fourth) GetTuple<TOut, TIn>(
				this TIn input,
				Func<TIn, TOut> firstSelector,
				Func<TIn, TOut> secondSelector,
				Func<TIn, TOut> thirdSelector,
				Func<TIn, TOut> fourthSelector)
		{
			return (
				firstSelector(input),
				secondSelector(input),
				thirdSelector(input),
				fourthSelector(input));
		}

		public static IEnumerable<DateTime> Range(this DateTime startDate, DateTime endDate, TimeSpan timeUnit)
		{
			var currentDate = startDate;

			while (currentDate < endDate)
			{
				currentDate += timeUnit;
				yield return currentDate;
			}
		}

		public static U Pipe<T, U>(this T input, Func<T, U> func)
		{
			return func(input);
		}

		public static bool IsWeekend(this DayOfWeek dayOfWeek) =>
			dayOfWeek is
				DayOfWeek.Saturday or
				DayOfWeek.Sunday;

		public static string GetDefaultFileName<T>(this T generator) where T : ISymbolsListGenerator
		{
			return generator.GetType().Name.Replace("SymbolsListGenerator", "");
		}
	}
}
