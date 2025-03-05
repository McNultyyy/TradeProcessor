using FluentResults;
using TradeProcessor.Domain.Exchange;

namespace TradeProcessor.Domain.Workers
{
	public class LimitAtTheOpen
	{
		private IExchangeRestClient _exchangeRestClient;

		public LimitAtTheOpen(IExchangeRestClient exchangeRestClient)
		{
			_exchangeRestClient = exchangeRestClient;
		}

		private async Task<Result> Open(Symbol symbol, BiasType biasType, DateTime dateTime, TimeSpan timeSpan)
		{
			var candles = await _exchangeRestClient.GetCandles(symbol, timeSpan, dateTime, dateTime + timeSpan);

			return Result.Ok();
		}

		public Task<Result> DailyOpen(Symbol symbol, BiasType biasType) =>
			Open(symbol, biasType, DateTime.UtcNow.Date, TimeSpan.FromDays(1));

		public Task<Result> WeeklyOpen(Symbol symbol, BiasType biasType)
		{
			var weeklyOpenDateTime = DateTime.UtcNow.Date
				.AddDays(((int)DateTime.UtcNow.DayOfWeek) - 1);

			return Open(symbol, biasType, weeklyOpenDateTime, TimeSpan.FromDays(1));
		}

		public Result MonthlyOpen(Symbol symbol, BiasType biasType)
		{
			// todo
			return null;
		}
	}
}
