namespace TradeProcessor.Domain.TakeProfit;

public class RiskRewardTakeProfit : ITakeProfit
{
    private readonly decimal _entryPrice;
    private readonly decimal _stoploss;
    private readonly decimal _riskReward;
    private readonly bool _isBullish;

    public RiskRewardTakeProfit(decimal entryPrice, decimal stoploss, decimal riskReward, bool isBullish)
    {
        _entryPrice = entryPrice;
        _stoploss = stoploss;
        _riskReward = riskReward;
        _isBullish = isBullish;
    }

    public decimal Result()
    {
        var diff = Math.Abs(_entryPrice - _stoploss);

        return _isBullish ?
            _entryPrice + _riskReward * diff :
            _entryPrice - _riskReward * diff;
    }
}
