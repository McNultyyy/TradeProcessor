namespace TradeProcessor.Domain.Candles;

//todo: remove the OpenDateTime = default
public record Candle(decimal Open, decimal High, decimal Low, decimal Close, DateTime OpenDateTime = default, DateTime CloseDateTime = default, Symbol Symbol = default) : ICandle
{
	public Candle(double Open, double High, double Low, double Close, DateTime OpenDateTime = default, DateTime CloseDateTime = default, Symbol Symbol = default)
		: this((decimal)Open, (decimal)High, (decimal)Low, (decimal)Close, OpenDateTime) { }

	public Candle(int Open, int High, int Low, int Close, DateTime OpenDateTime = default, DateTime CloseDateTime = default)
		: this((decimal)Open, (decimal)High, (decimal)Low, (decimal)Close, OpenDateTime) { }
}
