namespace TradeProcessor.Domain.Stoploss;

public class StaticStoploss : IStoploss
{
    private readonly decimal _result;

    public StaticStoploss(decimal result)
    {
        _result = result;
    }

    public decimal Result()
    {
        return _result;
    }
}
