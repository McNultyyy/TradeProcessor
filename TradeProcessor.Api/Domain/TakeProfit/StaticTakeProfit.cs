namespace TradeProcessor.Api.Domain.Stoploss;

public class StaticTakeProfit : ITakeProfit
{
    private decimal _result;

    public StaticTakeProfit(decimal result)
    {
        _result = result;
    }

    public decimal Result()
    {
        return _result;
    }
}
