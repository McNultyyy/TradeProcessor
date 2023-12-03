namespace TradeProcessor.Domain.Candles
{
	public interface ICanHaveImbalances
	{
		bool TryFindImbalances(out IEnumerable<Imbalance>? imbalances);
	}


	public record TwoCandles(ICandle Previous, ICandle Current) : ICanHaveImbalances
	{
		public bool TryFindImbalances(out IEnumerable<Imbalance>? imbalances)
		{
			var foundImbalances = new List<Imbalance>();


			TryFindOpeningImbalance(out var openingImbalance);
			if (openingImbalance is not null)
				foundImbalances.Add(openingImbalance);

			TryFindLiquidityImbalance(out var liquidityImbalance);
			if (liquidityImbalance is not null)
				foundImbalances.Add(liquidityImbalance);

			TryFindVolumeImbalance(out var volumeImbalance);
			if (volumeImbalance is not null)
				foundImbalances.Add(volumeImbalance);

			if (foundImbalances.Any())
			{
				imbalances = foundImbalances;
				return true;
			}

			imbalances = null;
			return false;
		}

		private bool TryFindOpeningImbalance(out Imbalance? imbalance)
		{
			if (Previous.IsBullishCandle() && Current.IsBullishCandle() &&
				Previous.Close > Current.Open)
			{
				imbalance = new Imbalance(Previous.Close, Current.Open, BiasType.Bullish, GapType.Opening);
				return true;
			}

			if (Previous.IsBearishCandle() && Current.IsBearishCandle() &&
				Previous.Close < Current.Open)
			{
				imbalance = new Imbalance(Current.Open, Previous.Close, BiasType.Bearish, GapType.Opening);
				return true;
			}

			imbalance = null;
			return false;
		}


		private bool TryFindLiquidityImbalance(out Imbalance? imbalance)
		{
			if (Previous.IsBullishCandle() && Current.IsBullishCandle() &&
				Previous.High < Current.Low)
			{
				imbalance = new Imbalance(Current.Low, Previous.High, BiasType.Bullish, GapType.Liquidity);
				return true;
			}

			if (Previous.IsBearishCandle() && Current.IsBearishCandle() &&
				Previous.Low > Current.High)
			{
				imbalance = new Imbalance(Previous.Low, Current.High, BiasType.Bearish, GapType.Liquidity);
				return true;
			}

			imbalance = null;
			return false;
		}

		private bool TryFindVolumeImbalance(out Imbalance? imbalance)
		{
			if (Current.IsBullishCandle() && Previous.IsBullishCandle() &&
				Current.Open > Previous.Close)
			{
				imbalance = new Imbalance(Current.Open, Previous.Close, BiasType.Bullish, GapType.Volume);
				return true;
			}

			if (Current.IsBearishCandle() && Previous.IsBearishCandle() &&
				Current.Open < Previous.Close)
			{
				imbalance = new Imbalance(Previous.Close, Current.Open, BiasType.Bearish, GapType.Volume);
				return true;
			}

			imbalance = null;
			return false;
		}
	}
}
