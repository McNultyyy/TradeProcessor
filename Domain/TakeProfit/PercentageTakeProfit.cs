namespace TradeProcessor.Domain.TakeProfit;

public class PercentageTakeProfit : ITakeProfit
{
	private readonly decimal _percentage;
	private readonly decimal _entryPrice;
	private readonly bool _isBullish;

	public PercentageTakeProfit(decimal percentage, decimal entryPrice, bool isBullish)
	{
		_percentage = percentage;
		_entryPrice = entryPrice;
		_isBullish = isBullish;
	}

	public decimal Result()
	{
		return _isBullish
			? _entryPrice * (100 + _percentage) / 100m
			: _entryPrice * (100 - _percentage) / 100m;
	}
}
