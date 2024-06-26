﻿using Bybit.Net.Enums;
using Bybit.Net.Interfaces.Clients;
using TradeProcessor.Domain;
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

		public async Task Subscribe(Symbol symbol, TimeSpan interval, Func<Candle, Task> handler)
		{
			var bybitKlineInterval = BybitHelper.MapToKlineInterval(interval);

			await _socketClient
				.DerivativesApi
				.SubscribeToKlineUpdatesAsync(StreamDerivativesCategory.USDTPerp, BybitHelper.ToBybitSymbol(symbol), bybitKlineInterval,
					async updates =>
					{

						var data = updates.Data.First();

						if (data.Confirm)
						{
							var candle = new Candle(data.OpenPrice, data.HighPrice, data.LowPrice, data.ClosePrice, data.OpenTime);

							await handler(candle);
						}
					});

			// todo: review the while(true) if things get weird
		}
	}
}
