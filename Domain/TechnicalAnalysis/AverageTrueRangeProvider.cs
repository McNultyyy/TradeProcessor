using Skender.Stock.Indicators;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.Exchange;

namespace TradeProcessor.Domain.TechnicalAnalysis
{
	public class AverageTrueRangeProvider
	{
		private readonly IExchangeRestClient _exchangeRestClient;

		private const int AtrLookbackPeriod = 14;

		public AverageTrueRangeProvider(IExchangeRestClient exchangeRestClient)
		{
			_exchangeRestClient = exchangeRestClient;
		}

		public async Task<decimal> GetCurrentAverageTrueRange(Symbol symbol, TimeSpan timeSpan)
		{
			var to = DateTime.UtcNow;
			var from = to - (timeSpan * AtrLookbackPeriod * 2); // times 2, just in case

			var candles = (await _exchangeRestClient.GetCandles(symbol, timeSpan, from, to)).Value;

			var atrs = candles.Select(ToQuote).GetAtr(AtrLookbackPeriod).Condense();

			//get most recent ATR
			return (decimal)atrs.MaxBy(x => x.Date).Atr.Value;
		}

		private Quote ToQuote(Candle candle)
		{
			return new Quote()
			{
				Open = candle.Open,
				High = candle.High,
				Low = candle.Low,
				Close = candle.Close,

				// todo: add volume??

				Date = candle.OpenDateTime,
			};
		}
	}
}
