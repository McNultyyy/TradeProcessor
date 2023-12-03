using ApiSharp.Extensions;
using FluentResults;
using OKX.Api;
using OKX.Api.Enums;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.Exchange;

namespace TradeProcessor.Infrastructure.Services.OKx
{
	public class OKxExchangeRestClient : IExchangeRestClient
	{
		private readonly OKXRestApiClient _restClient;

		public OKxExchangeRestClient(OKXRestApiClient restClient)
		{
			_restClient = restClient;
		}

		async Task<Result> IExchangeRestClient.PlaceOrder(string symbol, BiasType bias, decimal quantity, decimal price, decimal? takeProfit)
		{
			var orderResult = await _restClient.OrderBookTrading.Trade.PlaceOrderAsync(
				instrumentId: OKxHelper.ToOkxSymbol(symbol),
				tradeMode: OkxTradeMode.Cross,
				orderSide: bias == BiasType.Bullish ? OkxOrderSide.Buy : OkxOrderSide.Sell,
				positionSide: OkxPositionSide.Net ,
				orderType: OkxOrderType.LimitOrder,
				size: quantity,
				price: price);
			
			// todo: add take profits

			if (orderResult.Success)
				return Result.Ok();

			return Result.Fail(orderResult.Error.Message);
		}

		public async Task<Result<IEnumerable<Candle>>> GetCandles(string symbol, TimeSpan interval, DateTime from, DateTime to)
		{
			var okxPeriod = OKxHelper.MapToKlineInterval(interval);

			var result = await _restClient.OrderBookTrading.MarketData.GetCandlesticksAsync(
				OKxHelper.ToOkxSymbol(symbol),
				okxPeriod,
				to.ConvertToMilliseconds(),
				from.ConvertToMilliseconds()
				);

			if (result.Success)
			{
				var lastCandles = result
					.Data
					.Select(x =>
						new Candle(
							x.Open,
							x.High,
							x.Low,
							x.Close,
							x.Time,
							x.Time + interval,
							symbol));

				return Result.Ok(lastCandles);
			}
			else
				return Result.Fail(result.Error.Message);
		}
	}
}
