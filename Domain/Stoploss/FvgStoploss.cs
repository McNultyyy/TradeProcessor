namespace TradeProcessor.Domain.Stoploss
{
	public class FvgStoploss : IStoploss
	{
		private readonly decimal _fvgLow;
		private readonly decimal _fvgHigh;
		private readonly bool _isBullish;

		public FvgStoploss(decimal fvgLow, decimal fvgHigh, bool isBullish)
		{
			_fvgLow = fvgLow;
			_fvgHigh = fvgHigh;
			_isBullish = isBullish;
		}

		public decimal Result()
		{
			return _isBullish ? _fvgLow : _fvgHigh;
		}
	}
}
