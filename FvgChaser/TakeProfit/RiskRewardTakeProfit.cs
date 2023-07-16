namespace TradeProcessor.Api.FvgChaser.TakeProfit;

public class RiskRewardTakeProfit : ITakeProfit
{
    private decimal _entryPrice;
    private decimal _stoploss;
    private decimal _riskReward;
    private bool _isBullish;

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