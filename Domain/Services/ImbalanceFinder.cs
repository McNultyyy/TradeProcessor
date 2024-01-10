using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.DataProvider;

namespace TradeProcessor.Domain.Services
{
	public class ImbalanceFinder
	{
		private readonly IDataProvider _dataProvider;

		public ImbalanceFinder(IDataProvider dataProvider)
		{
			_dataProvider = dataProvider;
		}

		public async Task<IEnumerable<Imbalance>> FindImbalances(
			Symbol symbol,
			DateTime startDate, DateTime endDate,
			params GapType[] gapType)
		{

			var candles = (await _dataProvider.GetCandles(symbol, startDate, endDate))
				.OrderByDescending(x => x.OpenDateTime)
				.ToList();

			var imbalanceResults = new List<Imbalance>();

			for (int i = 2; i < candles.Count - 2; i++)
			{
				var (current, previous, previousPrevious) = (candles[i], candles[i - 1], candles[i - 2]);

				var threeCandles = new ThreeCandles(previousPrevious, previous, current);

				if (threeCandles.TryFindImbalances(out var imbalances))
				{
					if (gapType.Any())
						imbalances = imbalances.Where(x => gapType.Contains(x.GapType));

					imbalanceResults.AddRange(imbalances);
				}
			}

			return imbalanceResults;

		}
	}
}
