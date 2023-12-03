namespace TradeProcessor.Domain
{
	public interface IImbalance
	{
		decimal High { get; }
		decimal Low { get; }
		BiasType BiasType { get; }
		GapType GapType { get; }

	}
}
