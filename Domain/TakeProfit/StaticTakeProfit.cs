namespace TradeProcessor.Domain.TakeProfit;

public class StaticTakeProfit : ITakeProfit
{
    private readonly decimal _result;

    public StaticTakeProfit(decimal result)
    {
        _result = result;
    }

    public decimal Result()
    {
        return _result;
    }
}
