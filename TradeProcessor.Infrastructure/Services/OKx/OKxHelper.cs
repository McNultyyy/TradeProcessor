using OKX.Api.Enums;
using TradeProcessor.Domain.Helpers;

namespace TradeProcessor.Infrastructure.Services.OKx
{
	public static class OKxHelper
	{
		public static OkxPeriod MapToKlineInterval(string requestInterval)
		{
			return requestInterval switch
			{
				"1m" => OkxPeriod.OneMinute,
				"5m" => OkxPeriod.FiveMinutes,
				"15m" => OkxPeriod.FifteenMinutes,
				"1H" => OkxPeriod.OneHour,
				"4H" => OkxPeriod.FourHours,
				"1D" => OkxPeriod.OneDay,

				_ => throw new ArgumentOutOfRangeException(nameof(requestInterval), requestInterval)
			};
		}

		public static OkxPeriod MapToKlineInterval(TimeSpan timeSpan)
		{
			return MapToKlineInterval(
				TimeHelper.TimeSpanToIntervalString(timeSpan));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="symbol">BTCUSDT</param>
		/// <returns>BTC-USDT</returns>
		public static string ToOkxSymbol(string symbol)
		{
			var quoteCurrency = String.Join("", symbol.TakeLast("USDT".Length));

			var baseCurrency = symbol.Replace(quoteCurrency, "");

			return $"{baseCurrency}-{quoteCurrency}-SWAP";
		}
	}
}
