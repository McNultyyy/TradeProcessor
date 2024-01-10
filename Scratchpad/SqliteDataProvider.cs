using Scratchpad;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.DataProvider;

public class SqliteDataProvider : IDataProvider
{
	private readonly PricesContext _pricesContext;

	public SqliteDataProvider(PricesContext pricesContext)
	{
		_pricesContext = pricesContext;
	}

	public async Task<IEnumerable<ICandle>> GetCandles(Symbol symbol, DateTime from, DateTime to)
	{
		// todo: implement date filtering

		return _pricesContext.Candles
				.Where(x => x.Symbol == symbol)
				.Where(x =>
					x.OpenDateTime > from &&
					x.CloseDateTime < to);
	}
}
