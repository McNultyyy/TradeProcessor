namespace TradeProcessor.Api.Domain.Candles
{
	public record TwoCandles(Candle Previous, Candle Current)
	{
		public bool TryFindImbalance(out Imbalance? imbalance)
		{
			if (TryFindVolumeImbalance(out imbalance))
			{
				return true;
			}

			imbalance = null;
			return false;
		}

		private bool TryFindVolumeImbalance(out Imbalance? imbalance)
		{
			if (Current.IsBearishCandle() && Previous.IsBearishCandle() &&
				Current.Open < Previous.Close)
			{
				imbalance = new Imbalance(Previous.Close, Current.Open, BiasType.Bearish, GapType.Volume);
				return true;
			}

			if (Current.IsBullishCandle() && Previous.IsBullishCandle() &&
				Current.Open > Previous.Close)
			{
				imbalance = new Imbalance(Current.Open, Previous.Close, BiasType.Bullish, GapType.Volume);
				return true;
			}

			imbalance = null;
			return false;
		}
	}
}
