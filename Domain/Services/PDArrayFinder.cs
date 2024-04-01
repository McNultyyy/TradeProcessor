using System.ComponentModel;
using Skender.Stock.Indicators;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.Exchange;

namespace TradeProcessor.Domain.Services
{
	public class PDArrayFinder
	{
		private IExchangeRestClient _exchangeRestClient;

		public PDArrayFinder(IExchangeRestClient exchangeRestClient)
		{
			_exchangeRestClient = exchangeRestClient;
		}


		public async Task<List<(decimal price, BiasType biasType)>> Find(Symbol symbol, TimeSpan timeframe)
		{
			var dateFrom = DateTime.UtcNow - (200 * timeframe);
			var dateTo = DateTime.UtcNow;

			var candles = (await _exchangeRestClient.GetCandles(symbol, timeframe, dateFrom, dateTo)).Value;

			var pivotCandles = candles
				.Select(x => x.ToQuote())
				.GetFractal(2, EndType.HighLow).Condense()
				.OrderByDescending(x => x.Date);

			var untappedLevels = new List<FractalResult>();

			foreach (var pivotCandle in pivotCandles)
			{
				var candlesAfter = candles
						.Where(x => x.OpenDateTime > (pivotCandle.Date))
						.Where(x =>
						{
							if (pivotCandle.FractalBull is not null &&
							    x.Low < pivotCandle.FractalBull)
								return true;

							if (pivotCandle.FractalBear is not null &&
							    x.High > pivotCandle.FractalBear)
								return true;

							return false;
						})
						.ToList()
					;

				if (!candlesAfter.Any())
				{
					untappedLevels.Add(pivotCandle);
				}
			}

			return untappedLevels
				.Select(x => (
					(x.FractalBear ?? x.FractalBull).Value,
					x.FractalBear is null ? BiasType.Bullish : BiasType.Bearish
				))
				.ToList();
		}
	}


	public enum PDArray
	{
		HighLow,
		FVG,
		VIB,
	}
}
