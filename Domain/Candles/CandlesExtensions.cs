namespace TradeProcessor.Domain.Candles
{
	public static class CandlesExtensions
	{
		public static IEnumerable<Pivot> GetPivots(this IEnumerable<ICandle> enumerableCandles)
		{
			// todo: make the numberOfCandlesToConsiderAPivot ACTUALLY configurable
			var numberOfCandlesToConsiderAPivot = 3;
			var candles = enumerableCandles.ToList();

			for (int i = 2; i < candles.Count() - numberOfCandlesToConsiderAPivot; i++)
			{
				var previousPrevious = candles[i - 2];
				var previous = candles[i - 1];
				var current = candles[i];

				if (Pivot.TryCreatePivot(previousPrevious, previous, current, out var pivot))
					yield return pivot;
			}
		}

		public static IEnumerable<SequenceOfCandles> GetSequencesStrict(this IEnumerable<ICandle> candles)
		{
			var listOfCandles = new List<ICandle>();
			var sequences = new List<SequenceOfCandles>();

			foreach (var currentCandle in candles)
			{
				if (listOfCandles.Count == 0)
					listOfCandles.Add(currentCandle);
				else
				{
					/*
					 * todo: instead of only adding candles in the same direction.
					 * consider all consequtive candles which do not close below 50% of the previous
					 */
					if (listOfCandles.First().IsInSameDirection(currentCandle))
					{
						listOfCandles.Add(currentCandle);
					}
					else
					{
						sequences.Add(new SequenceOfCandles(listOfCandles.ToArray()));
						listOfCandles.Clear();
						listOfCandles.Add(currentCandle);
					}
				}
			}

			return sequences;
		}

		public static IEnumerable<SequenceOfCandles> GetSequences(this IEnumerable<Candle> candles)
		{
			var listOfCandles = new List<Candle>();
			var sequences = new List<SequenceOfCandles>();

			foreach (var currentCandle in candles)
			{
				if (listOfCandles.Count == 0)
					listOfCandles.Add(currentCandle);
				else
				{
					// 
					if (listOfCandles.Any(x =>
							currentCandle.IsBullishCandle() ?
								currentCandle.ClosesBelowOpen(x) :
								currentCandle.ClosesAboveOpen(x)))
					{
						sequences.Add(new SequenceOfCandles(listOfCandles.ToArray()));
						listOfCandles.Clear();
						listOfCandles.Add(currentCandle);
					}
					else
					{
						listOfCandles.Add(currentCandle);
					}
				}
			}

			return sequences;
		}
	}
}
