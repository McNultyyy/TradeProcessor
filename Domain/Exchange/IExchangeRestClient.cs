using FluentResults;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.Services;

namespace TradeProcessor.Domain.Exchange
{
	public interface IExchangeRestClient
	{
		public Task<Result> PlaceOrder(TradeTicket trade);

		public Task<Result<IEnumerable<Candle>>> GetCandles(Symbol symbol, TimeSpan interval, DateTime from, DateTime to);

		public Task<Result<IEnumerable<Symbol>>> GetSymbols();

		public Task<Result> EnsureMaxCrossLeverage(Symbol symbol);

		public Task<Result<decimal>> GetAccountBalance();

		public Task<Result> CancelAllOrders();
	}
}
