using OKX.Api.Enums;
using TradeProcessor.Domain;
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
				"30m" => OkxPeriod.ThirtyMinutes,
				"60m" or "1H" => OkxPeriod.OneHour,
				"4H" => OkxPeriod.FourHours,
				"24H" or "1D" => OkxPeriod.OneDay,
				"7D" or "1W" => OkxPeriod.OneWeek,

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
		public static string ToOkxSymbol(Symbol symbol)
		{
			return $"{symbol.Base}-{symbol.Quote}-SWAP";
		}

		public static Symbol ToSymbol(string okxSymbol)
		{
			var parts = okxSymbol.Split("-");

			// todo: refactor?
			// kinda hacky but we always assume that any USD denominated pair is actually USDT
			var quote = okxSymbol.Contains("USD-SWAP") ? "USDT" : parts[1];

			return new Symbol(parts[0], quote);
		}
	}
}
