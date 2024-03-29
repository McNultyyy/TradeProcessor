﻿namespace TradeProcessor.Domain.Stoploss;

public class RelativeStoploss : IStoploss
{
    private readonly decimal _entryPrice;
    private readonly decimal _offset;
    private readonly bool _isBullish;

    public RelativeStoploss(decimal entryPrice, decimal offset, bool isBullish)
    {
        _entryPrice = entryPrice;
        _offset = offset; //todo: should we Math.Abs this
        _isBullish = isBullish;
    }

    public decimal Result()
    {
        return _isBullish ?
            _entryPrice - _offset :
            _entryPrice + _offset;
    }
}
