using Bybit.Net.Enums;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Helpers;

namespace TradeProcessor.Infrastructure.Services.Bybit
{
	public static class BybitHelper
	{
		public static KlineInterval MapToKlineInterval(string requestInterval)
		{
			if (requestInterval.Contains("m"))
			{
				requestInterval = requestInterval.Replace("m", "");
				var integer = int.Parse(requestInterval);

				var integerInSeconds = integer * 60;

				return (KlineInterval)integerInSeconds;
			}

			if (requestInterval.Contains("H", StringComparison.InvariantCultureIgnoreCase))
			{
				requestInterval = requestInterval.Replace("H", "");
				var integer = int.Parse(requestInterval);

				var integerInSeconds = integer * 60 * 60;

				return (KlineInterval)integerInSeconds;
			}

			if (requestInterval.Contains("D"))
			{
				requestInterval = requestInterval.Replace("D", "");
				var integer = int.Parse(requestInterval);

				var integerInSeconds = integer * 60 * 60 * 24;

				return (KlineInterval)integerInSeconds;
			}

			throw new ArgumentException($"Cannot parse {requestInterval}", nameof(requestInterval));
		}

		public static KlineInterval MapToKlineInterval(TimeSpan timeSpan)
		{
			return MapToKlineInterval(
				TimeHelper.TimeSpanToIntervalString(timeSpan));
		}

		public static string ToBybitSymbol(Symbol symbol)
		{
			return $"{symbol.Base}{symbol.Quote}";
		}
	}
}
