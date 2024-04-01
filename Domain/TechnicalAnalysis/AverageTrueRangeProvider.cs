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

			var atrs = candles.Select(x => x.ToQuote()).GetAtr(AtrLookbackPeriod).Condense();

			//get most recent ATR
			var atr = (decimal)atrs.MaxBy(x => x.Date).Atr.Value;
			return atr;
		}
	}
}
