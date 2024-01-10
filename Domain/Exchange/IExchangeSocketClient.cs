using TradeProcessor.Domain.Candles;

namespace TradeProcessor.Domain.Exchange
{
	public interface IExchangeSocketClient
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="symbol"></param>
		/// <param name="interval">Interval in the format of 5m, 1h, 1d etc...</param>
		/// <param name="handler"></param>
		/// <returns></returns>
		Task Subscribe(Symbol symbol, TimeSpan interval, Func<Candle, Task> handler);
	}
}
