namespace TradeProcessor.Domain.Helpers
{
	public static class TimeHelper
	{
		public static string TimeSpanToIntervalString(TimeSpan timeSpan)
		{
			if (timeSpan < TimeSpan.FromMinutes(61))
				return $"{timeSpan.Minutes}m";

			if (timeSpan < TimeSpan.FromHours(25))
				return $"{timeSpan.Hours}H";

			if (timeSpan < TimeSpan.FromDays(8))
				return $"{timeSpan.Days}D";

			throw new ArgumentOutOfRangeException(nameof(timeSpan), timeSpan.ToString());
		}

		public static TimeSpan IntervalStringToTimeSpan(string requestInterval)
		{
			if (requestInterval.Contains("m"))
			{
				requestInterval = requestInterval.Replace("m", "");
				var integer = int.Parse(requestInterval);

				return TimeSpan.FromMinutes(integer);
			}

			if (requestInterval.Contains("H", StringComparison.InvariantCultureIgnoreCase))
			{
				requestInterval = requestInterval.Replace("H", "");
				var integer = int.Parse(requestInterval);

				return TimeSpan.FromHours(integer);
			}

			if (requestInterval.Contains("D"))
			{
				requestInterval = requestInterval.Replace("D", "");
				var integer = int.Parse(requestInterval);

				return TimeSpan.FromDays(integer);
			}

			throw new ArgumentException($"Cannot parse {requestInterval}", nameof(requestInterval));
		}
	}
}
