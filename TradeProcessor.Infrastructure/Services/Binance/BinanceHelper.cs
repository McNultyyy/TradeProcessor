using OKX.Api.Enums;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Helpers;

namespace TradeProcessor.Infrastructure.Services.Binance
{
	public static class BinanceHelper
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
		public static string ToBinanceSymbol(Symbol symbol)
		{
			return $"{symbol.Base}{symbol.Quote}";
		}

		public static Symbol ToSymbol(string okxSymbol)
		{
			var quote = okxSymbol.EndsWith("USDT")
				? "USDT"
				: okxSymbol.EndsWith("BTC")
					? "BTC"
					: okxSymbol.EndsWith("ETH")
						? "ETH"
						: "";
			var parts = okxSymbol.Split(quote);

			// todo: refactor?
			// kinda hacky but we always assume that any USD denominated pair is actually USDT


			return new Symbol(parts[0], quote);
		}
	}
}
