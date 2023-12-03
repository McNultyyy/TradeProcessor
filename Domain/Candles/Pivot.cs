namespace TradeProcessor.Domain.Candles
{
	public enum PivotType
	{
		High,
		Low
	}

	public record Pivot(ICandle PivotCandle, PivotType PivotType)
	{
		public static bool TryCreatePivot(ICandle previousPrevious, ICandle previous, ICandle current, out Pivot? pivot)
		{
			pivot = null;

			if (previousPrevious.High < previous.High &&
				current.High < previous.High)
				pivot = new Pivot(previous, PivotType.High);

			if (previousPrevious.Low > previous.Low &&
				current.Low > previous.Low)
				pivot = new Pivot(previous, PivotType.Low);

			return pivot is not null;
		}
	}
}
