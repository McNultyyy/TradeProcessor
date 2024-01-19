using OKX.Api;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.Exchange;

namespace TradeProcessor.Infrastructure.Services.OKx
{
	public class OkxExchangeSocketClient : IExchangeSocketClient
	{
		private readonly OKXWebSocketApiClient _socketClient;

		public OkxExchangeSocketClient(OKXWebSocketApiClient socketClient)
		{
			_socketClient = socketClient;
		}

		public async Task Subscribe(Symbol symbol, TimeSpan interval, Func<Candle, Task> handler)
		{
			var okxPeriod = OKxHelper.MapToKlineInterval(interval);
			var okxSymbol = OKxHelper.ToOkxSymbol(symbol);

			await _socketClient
				.OrderBookTrading.MarketData.SubscribeToCandlesticksAsync(async candlestick =>
				{
					if (candlestick.Confirm)
					{
						var candle = new Candle(candlestick.Open, candlestick.High, candlestick.Low, candlestick.Close,
							candlestick.Time);

						await handler(candle);
					}
				},
				okxSymbol,
				okxPeriod);


			/*
			 * We want to keep this instance alive so that we can:
			 *  1. Stop the job, if necessary, via the Hangfire UI.
			 *  2. So that the REST and Socket clients do not get disposed until the job is manually killed
			 */

			while (true) { }
		}

		public void Dispose()
		{
			_socketClient.Dispose();
		}
	}
}
