namespace TradeProcessor.Domain.Helpers
{
	public static class TimeHelper
	{
		public static string TimeSpanToIntervalString(TimeSpan timeSpan)
		{
			if (timeSpan < TimeSpan.FromMinutes(61))
				return $"{timeSpan.TotalMinutes}m";

			if (timeSpan < TimeSpan.FromHours(25))
				return $"{timeSpan.TotalHours}H";

			if (timeSpan < TimeSpan.FromDays(8))
				return $"{timeSpan.TotalDays}D";

			// todo: revisit this - kinda dodgy logic
			if (timeSpan < TimeSpan.FromDays(32))
				return $"{timeSpan.TotalDays / 7}W";

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

			// todo: unsure if this works how we expect
			if (requestInterval.Contains("W"))
			{
				requestInterval = requestInterval.Replace("W", "");
				var integer = int.Parse(requestInterval);

				return TimeSpan.FromDays(integer * 7);
			}

			throw new ArgumentException($"Cannot parse {requestInterval}", nameof(requestInterval));
		}
	}
}
