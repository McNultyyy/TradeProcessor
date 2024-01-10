namespace TradeProcessor.Domain.Candles
{
	public record OrderBlock(
		ICandle LiquidityCandle,
		ICandle LiquidityTakingCandle,
		ICandle ImpulseCandle)
		: ICandle
	{
		public Symbol Symbol { get; set; } = LiquidityCandle.Symbol;

		/*
		 * todo:
		 * if the wick High/Low has a larger range than the Open - Close, then use the High/Low
		 * as there is likely an OrderBlock within the wick
		 */
		public decimal Open { get; } = LiquidityTakingCandle.Open;
		public decimal High { get; } = Math.Max(ImpulseCandle.High, LiquidityTakingCandle.High);
		public decimal Low { get; } = Math.Min(ImpulseCandle.Low, LiquidityTakingCandle.Low);
		public decimal Close { get; } = LiquidityTakingCandle.Close;
		public DateTime OpenDateTime { get; } = LiquidityTakingCandle.OpenDateTime;
		public DateTime CloseDateTime { get; } = ImpulseCandle.CloseDateTime;
	}
}
