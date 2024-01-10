using FluentResults;

namespace TradeProcessor.Domain.Candles
{
	public static class CandleFactory
	{
		public static Result<Candle> CreateMonthlyCandle(IEnumerable<Candle> candles)
		{
			if (!candles.All(x => x.OpenDateTime.Month == candles.First().OpenDateTime.Month))
				return Result.Fail("Not all candles share the same month");


			var openCandle = candles.MinBy(x => x.OpenDateTime);
			var closeCandle = candles.MaxBy(x => x.CloseDateTime);

			decimal open, high, low, close;

			open = openCandle.Open;
			high = candles.Max(x => x.High);
			low = candles.Min(x => x.Low);
			close = closeCandle.Close;

			return Result.Ok(new Candle(open, high, low, close, openCandle.OpenDateTime, closeCandle.CloseDateTime, openCandle.Symbol));
		}

		public static Result<IEnumerable<Candle>> CreateMonthlyCandles(IEnumerable<Candle> candles)
		{
			var monthYearGroups = candles.GroupBy(x => (x.OpenDateTime.Month, x.OpenDateTime.Year));

			var monthlyCandles = new List<Candle>();

			foreach (var monthYearGroup in monthYearGroups)
			{
				var monthlyCandle = CreateMonthlyCandle(monthYearGroup).Value;
				monthlyCandles.Add(monthlyCandle);
			}

			return Result.Ok(monthlyCandles.OrderByDescending(x => x.OpenDateTime).AsEnumerable());
		}
	}
}
