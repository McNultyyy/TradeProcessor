using Bybit.Net.Enums;
using Bybit.Net.Interfaces.Clients;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.Exchange;

namespace TradeProcessor.Infrastructure.Services.Bybit
{
	public class BybitExchangeSocketClient : IExchangeSocketClient
	{

		private readonly IBybitSocketClient _socketClient;

		public BybitExchangeSocketClient(IBybitSocketClient socketClient)
		{
			_socketClient = socketClient;
		}

		public async Task Subscribe(string symbol, TimeSpan interval, Func<Candle, Task> handler)
		{
			var bybitKlineInterval = BybitHelper.MapToKlineInterval(interval);

			await _socketClient
				.DerivativesApi
				.SubscribeToKlineUpdatesAsync(StreamDerivativesCategory.USDTPerp, symbol, bybitKlineInterval,
					async updates =>
					{

						var data = updates.Data.First();

						if (data.Confirm)
						{
							var candle = new Candle(data.OpenPrice, data.HighPrice, data.LowPrice, data.ClosePrice, data.OpenTime);

							await handler(candle);
						}
					});

			while (true)
			{
				/*
				 * We want to keep this instance alive so that we can:
				 *  1. Stop the job, if necessary, via the Hangfire UI.
				 *  2. So that the REST and Socket clients do not get disposed.
				 */
			}
		}
	}
}
