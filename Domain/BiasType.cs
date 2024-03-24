using System.Reflection.Metadata.Ecma335;

namespace TradeProcessor.Domain;

public enum BiasType
{
	Bullish,
	Bearish
}

public static class BiasTypeExtensions
{
	public static bool IsBullish(this BiasType type) => type is BiasType.Bullish;
}

public enum GapType
{
	Price,
	Volume,
	Liquidity,
	Opening
}
