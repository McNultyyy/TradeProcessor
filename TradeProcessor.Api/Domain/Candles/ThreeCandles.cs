namespace TradeProcessor.Api.Domain.Candles
{
	public record ThreeCandles(Candle PreviousPrevious, Candle Previous, Candle Current)
	{
		public bool TryFindImbalance(out Imbalance? imbalance)
		{

			if (TryFindVolumeImbalance(out imbalance))
			{
				return true;
			}

			if (TryFindPriceImbalance(out imbalance))
			{
				return true;
			}

			imbalance = null;
			return false;
		}

		private bool TryFindVolumeImbalance(out Imbalance? imbalance)
		{
			var twoCandles = new TwoCandles(Previous, Current);
			var previousTwoCandles = new TwoCandles(PreviousPrevious, Previous);


			if (previousTwoCandles.TryFindImbalance(out imbalance)) return true;
			if (twoCandles.TryFindImbalance(out imbalance)) return true;

			imbalance = null;
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
