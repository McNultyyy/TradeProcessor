namespace TradeProcessor.Api.FvgChaser;

public static class CandleExtensions
{
    public static bool IsBullishCandle(this Candle candle)
    {
        return candle.Close > candle.Open;
    }

    public static bool IsBearishCandle(this Candle candle)
    {
        return candle.Close < candle.Open;
    }
}