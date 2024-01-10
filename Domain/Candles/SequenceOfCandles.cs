namespace TradeProcessor.Domain.Candles
{
	public record SequenceOfCandles : ICandle
	{
		public Symbol Symbol { get; set; }
		public decimal Open { get; }
		public decimal High { get; }
		public decimal Low { get; }
		public decimal Close { get; }
		public DateTime OpenDateTime { get; }
		public DateTime CloseDateTime { get; }

		private readonly IEnumerable<ICandle> _innerCandles;

		public SequenceOfCandles(params ICandle[] candles)
		{
			_innerCandles = candles.AsEnumerable();

			Symbol = candles.First().Symbol;

			Open = candles.OrderBy(x => x.OpenDateTime).First().Open;
			High = candles.Max(x => x.High);
			Low = candles.Min(x => x.Low);
			Close = candles.OrderByDescending(x => x.OpenDateTime).First().Close;

			OpenDateTime = candles.OrderBy(x => x.OpenDateTime).First().OpenDateTime;
			CloseDateTime = candles.OrderByDescending(x => x.CloseDateTime).First().CloseDateTime;
		}

		public bool ContainsCandle(ICandle candle)
		{
			return _innerCandles.Contains(candle);
		}
	}
}
