namespace TradeProcessor.Api.FvgChaser.TakeProfit;

public class RelativeTakeProfit : ITakeProfit
{
    private readonly decimal _entryPrice;
    private decimal _offset;
    private readonly bool _isBullish;

    public RelativeTakeProfit(decimal entryPrice, decimal offset, bool isBullish)
    {
        _entryPrice = entryPrice;
        _offset = offset;
        _isBullish = isBullish;
    }

    public decimal Result()
    {
        return _isBullish ?
            _entryPrice + _offset :
            _entryPrice - _offset;
    }
}