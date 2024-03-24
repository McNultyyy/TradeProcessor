namespace TradeProcessor.Domain.TakeProfit
{
	public class FvgTakeProfit : ITakeProfit
	{
		private readonly decimal _fvgLow;
		private readonly decimal _fvgHigh;
		private readonly bool _isBullish;

		public FvgTakeProfit(decimal fvgLow, decimal fvgHigh, bool isBullish)
		{
			_fvgLow = fvgLow;
			_fvgHigh = fvgHigh;
			_isBullish = isBullish;
		}

		public decimal Result()
		{
			return _isBullish ? _fvgHigh : _fvgLow;
		}
	}
}
