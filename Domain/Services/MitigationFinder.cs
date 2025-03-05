using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.DataProvider;

namespace TradeProcessor.Domain.Services
{
	public class MitigationFinder
	{
		private readonly IDataProvider _dataProvider;

		public MitigationFinder(IDataProvider dataProvider)
		{
			_dataProvider = dataProvider;
		}

		public async Task<OrderBlock?> FindMitigationBlocks(
			Symbol symbol,
			DateTime startDate, DateTime endDate,
			TimeSpan interval,
			BiasType bias)
		{
			OrderBlock? orderBlock;

			while ((orderBlock = await TryFindOrderBlock(symbol, startDate, endDate, interval, bias)) is null
				   &&
				   startDate < endDate)
			{
				startDate += interval;
			};

			return orderBlock;
		}


		private async Task<OrderBlock?> TryFindOrderBlock(Symbol symbol, DateTime startDate, DateTime endDate, TimeSpan interval, BiasType bias)
		{
			try
			{
				var candles = (await _dataProvider.GetCandles(symbol, startDate, endDate))
					.OrderBy(x => x.OpenDateTime);

				// todo
				// abstract into `Liquidity`.
				// then include things like equal highs/lows etc.
				// also include filling FVGs?

				var pivots = candles.GetPivots()
					.Where(x =>
						x.PivotType == (bias is BiasType.Bearish ?
							PivotType.High :
							PivotType.Low))
					.ToList();

				var sequences = candles.GetSequencesStrict();

				var firstPivot = pivots.First();

				var firstCandleThatBreaksPivot = candles
					.Where(candle => candle.IsAfter(firstPivot.PivotCandle))
					.First(candle =>
						(bias is BiasType.Bearish
							? candle.HasHigherHighThan(firstPivot.PivotCandle) && candle.IsBullishCandle()
							: candle.HasLowerLowThan(firstPivot.PivotCandle) && candle.IsBearishCandle()));

				var sequenceWhichBrokeThePivot = sequences
					.First(sequence =>
						sequence.ContainsCandle(firstCandleThatBreaksPivot)
					);

				var firstCloseThatBreaksSequenceOpen = candles
					.Where(candle => candle.IsAfter(sequenceWhichBrokeThePivot))
					.First(candle =>
						bias is BiasType.Bearish
							? candle.ClosesBelowOpen(sequenceWhichBrokeThePivot)
							: candle.ClosesAboveOpen(sequenceWhichBrokeThePivot));


				var firstSequenceThatBreaksSequenceOpen = sequences
					.First(sequence =>
						sequence.ContainsCandle(firstCloseThatBreaksSequenceOpen)
					);

				var orderBlock = new OrderBlock(
					firstPivot.PivotCandle,
					sequenceWhichBrokeThePivot,
					firstSequenceThatBreaksSequenceOpen);

				return orderBlock;
			}
			catch (Exception ex)
			{
				return null;
			}
		}
	}
}
