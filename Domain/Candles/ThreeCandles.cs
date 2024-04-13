namespace TradeProcessor.Domain.Candles
{
	public record ThreeCandles(ICandle PreviousPrevious, ICandle Previous, ICandle Current) : ICanHaveImbalances
	{
		public bool TryFindImbalances(out IEnumerable<Imbalance>? imbalances)
		{

			var foundImbalances = new List<Imbalance>();

			var twoCandles = new TwoCandles(Previous, Current);
			if (twoCandles.TryFindImbalances(out var firstImbalances) && firstImbalances is not null)
				foundImbalances.AddRange(firstImbalances);

			var previousTwoCandles = new TwoCandles(PreviousPrevious, Previous);
			if (previousTwoCandles.TryFindImbalances(out var secondImbalances) && secondImbalances is not null)
				foundImbalances.AddRange(secondImbalances);

			if (TryFindPriceImbalance(out var priceImbalance) && priceImbalance is not null)
				foundImbalances.Add(priceImbalance);


			if (foundImbalances.Any())
			{
				imbalances = foundImbalances;
				return true;
			}

			imbalances = null;
			return false;
		}


		private bool TryFindPriceImbalance(out Imbalance? imbalance)
		{
			if (
				Previous.IsBearishCandle() &&
				PreviousPrevious.Low > Current.High
			)
			{
				imbalance = new Imbalance(PreviousPrevious.Low, Current.High, BiasType.Bearish, GapType.Price);
				return true;
			}

			if (
				Previous.IsBullishCandle() &&
				PreviousPrevious.High < Current.Low
			)
			{
				imbalance = new Imbalance(Current.Low, PreviousPrevious.High, BiasType.Bullish, GapType.Price);
				return true;
			}

			imbalance = null;
			return false;
		}


	}
}
