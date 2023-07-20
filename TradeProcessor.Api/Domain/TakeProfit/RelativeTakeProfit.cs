namespace TradeProcessor.Api.Domain.Stoploss;

public class RelativeTakeProfit : ITakeProfit
{
    private readonly decimal _entryPrice;
    private decimal _offset;
    private readonly bool _isBullish;

    public RelativeTakeProfit(decimal entryPrice, decimal offset, bool isBullish)
    {
        _entryPrice = entryPrice;
        _offset = offset; //todo: should we Math.Abs this
        _isBullish = isBullish;
    }

    public decimal Result()
    {
        return _isBullish ?
            _entryPrice + _offset :
            _entryPrice - _offset;
    }
}
