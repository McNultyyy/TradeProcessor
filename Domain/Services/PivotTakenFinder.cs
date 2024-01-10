using System.Globalization;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.DataProvider;

namespace TradeProcessor.Domain.Services
{
	public enum TimeRange
	{
		Daily,
		Weekly,
		Monthly
	}

	public class PivotTakenFinder
	{
		private readonly IDataProvider _dataProvider;

		public PivotTakenFinder(IDataProvider dataProvider)
		{
			_dataProvider = dataProvider;
		}

		public async Task<IEnumerable<string>> FindPivotsTaken(
			Symbol symbol,
			TimeRange timeSpan)
		{

			var candles = (await _dataProvider.GetCandles(
					symbol,
					DateTime.ParseExact("2021-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture),
					DateTime.UtcNow))
				.OrderByDescending(x => x.OpenDateTime)
				.ToList();

			if (timeSpan is TimeRange.Monthly)
			{
				var monthlyCandles = candles
					.GroupBy(x => x.OpenDateTime.ToString("yy-MM"))
					.Select(x =>
					{
						return new Candle(
							x.OrderBy(y => y.OpenDateTime).First().Open,
							x.Max(y => y.High),
							x.Min(y => y.Low),
							x.OrderBy(y => y.OpenDateTime).Last().Close,
							x.OrderBy(y => y.OpenDateTime).First().OpenDateTime,
							x.OrderBy(y => y.OpenDateTime).Last().CloseDateTime,
							symbol
						);
					})
					.OrderBy(x => x.OpenDateTime)
					.ToList();

				var untappedLevels = new List<ICandle>();
				foreach (var candle in monthlyCandles)
				{
					var candlesAfter = monthlyCandles
						.Where(x => x.IsAfter(candle))
						.Where(x => x.Low < candle.Low || x.High > candle.High)
						.ToList()
						;

					if (!candlesAfter.Any())
					{
						untappedLevels.Add(candle);
					}

				}

				var g = "";
			}

			return null;
		}
	}
}
