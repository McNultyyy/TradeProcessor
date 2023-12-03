using FluentResults;
using TradeProcessor.Domain.Candles;

namespace TradeProcessor.Domain.Exchange
{
	public interface IExchangeRestClient
	{
		public Task<Result> PlaceOrder(string symbol, BiasType bias, decimal quantity, decimal price, decimal? takeProfit);

		public Task<Result<IEnumerable<Candle>>> GetCandles(string symbol, TimeSpan interval, DateTime from, DateTime to);
	}
}
