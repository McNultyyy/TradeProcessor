namespace TradeProcessor.Api.Domain;

public record Candle(decimal Open, decimal High, decimal Low, decimal Close)
{
	public Candle(double Open, double High, double Low, double Close)
		: this((decimal)Open, (decimal)High, (decimal)Low, (decimal)Close) { }

	public Candle(int Open, int High, int Low, int Close)
		: this((decimal)Open, (decimal)High, (decimal)Low, (decimal)Close) { }

	public bool IsBullishCandle()
	{
		return Close > Open;
	}

	public bool IsBearishCandle()
	{
		return Close < Open;
	}
}
