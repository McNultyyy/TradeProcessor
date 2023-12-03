using TradeProcessor.Domain.Candles;

namespace TradeProcessor.Domain.DataProvider;

public interface IDataProvider
{
	Task<IEnumerable<ICandle>> GetCandles(string symbol, DateTime from, DateTime to);
}
