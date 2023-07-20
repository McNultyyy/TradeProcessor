namespace TradeProcessor.Api.Domain;

public record Candle(decimal Open, decimal High, decimal Low, decimal Close)
{
	public bool IsBullishCandle()
	{
		return Close > Open;
	}

	public bool IsBearishCandle()
	{
		return Close < Open;
	}
}
