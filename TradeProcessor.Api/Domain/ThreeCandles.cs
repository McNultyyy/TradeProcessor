namespace TradeProcessor.Api.Domain
{
	public record ThreeCandles(Candle PreviousPrevious, Candle Previous, Candle Current)
	{
		public bool TryFindImbalance(out Imbalance? imbalance)
		{

			if (
				Previous.IsBearishCandle() &&
				PreviousPrevious.Low > Current.High
				)
			{
				imbalance = new Imbalance(PreviousPrevious.Low, Current.High, ImbalanceType.Bearish);
				return true;
			}

			if (
				Previous.IsBullishCandle() &&
				PreviousPrevious.High < Current.Low
				)
			{
				imbalance = new Imbalance(Current.Low, PreviousPrevious.High, ImbalanceType.Bullish);
				return true;
			}

			imbalance = null;
			return false;
		}
	}
}
