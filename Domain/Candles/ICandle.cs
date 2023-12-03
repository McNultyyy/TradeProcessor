namespace TradeProcessor.Domain.Candles
{
	public interface ICandle
	{
		string Symbol { get; }

		decimal Open { get; }
		decimal High { get; }
		decimal Low { get; }
		decimal Close { get; }
		DateTime OpenDateTime { get; }
		DateTime CloseDateTime { get; }
	}
}
