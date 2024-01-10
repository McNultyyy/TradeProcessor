using TradeProcessor.Domain.Candles;

namespace TradeProcessor.Domain.DataProvider;

public interface IDataProvider
{
	Task<IEnumerable<ICandle>> GetCandles(Symbol symbol, DateTime from, DateTime to);
}
