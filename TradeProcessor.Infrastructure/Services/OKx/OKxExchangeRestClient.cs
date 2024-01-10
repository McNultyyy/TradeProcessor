﻿using ApiSharp.Extensions;
using ApiSharp.Models;
using FluentResults;
using OKX.Api;
using OKX.Api.Enums;
using OKX.Api.Models.MarketData;
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

		async Task<Result> IExchangeRestClient.PlaceOrder(Symbol symbol, BiasType bias, decimal quantity, decimal price, decimal? takeProfit)
		{

			decimal? tpOrderTriggerPrice = null;
			if (takeProfit is not null)
			{
				/*
				 * OKx has an awkward API and doesn't allow us to set the TP at the exact time of creating the trade.
				 * So when price reaches 30% of the way from our entry to the profit target,
				 * we should place our limit order.
				 */

				var diff = Math.Abs(takeProfit.Value - price);
				var percentageOfDiff = diff * 0.30m;

				tpOrderTriggerPrice = bias == BiasType.Bullish ?
					price + percentageOfDiff :
					price - percentageOfDiff;
			}

			var orderResult = await _restClient.OrderBookTrading.Trade.PlaceOrderAsync(
				instrumentId: OKxHelper.ToOkxSymbol(symbol),
				tradeMode: OkxTradeMode.Cross,
				orderSide: bias == BiasType.Bullish ? OkxOrderSide.Buy : OkxOrderSide.Sell,
				positionSide: OkxPositionSide.Net,
				orderType: OkxOrderType.LimitOrder,
				size: quantity,
				price: price,
				tpOrdPx: takeProfit,
				tpTriggerPx: tpOrderTriggerPrice);

			// todo: add take profits

			if (orderResult.Success)
				return Result.Ok();

			return Result.Fail(orderResult.Error.Message);
		}

		public async Task<Result<IEnumerable<Candle>>> GetCandles(Symbol symbol, TimeSpan interval, DateTime from, DateTime to)
		{
			var okxPeriod = OKxHelper.MapToKlineInterval(interval);

			var result = await _restClient.OrderBookTrading.MarketData.GetCandlesticksAsync(
				OKxHelper.ToOkxSymbol(symbol),
				okxPeriod,
				to.ConvertToMilliseconds(),
				from.ConvertToMilliseconds(),
				limit: 300
				);

			if (result.Success)
			{
				var lastCandles = ConvertToCandles(symbol, interval, result);

				var earliestCandleDate = lastCandles.Min(x => x.OpenDateTime);

				// todo: make this recursive ?
				if (earliestCandleDate > from)
				{
					var result2 = await _restClient.OrderBookTrading.MarketData.GetCandlesticksAsync(
						OKxHelper.ToOkxSymbol(symbol),
						okxPeriod,
						earliestCandleDate.ConvertToMilliseconds(),
						before: null,
						limit: 300
					);

					var previousLastCandles = ConvertToCandles(symbol, interval, result2);

					return Result.Ok(previousLastCandles.Concat(lastCandles));
				}

				return Result.Ok(lastCandles);
			}
			else
				return Result.Fail(result.Error.Message);
		}

		private static IEnumerable<Candle> ConvertToCandles(Symbol symbol, TimeSpan interval, RestCallResult<IEnumerable<OkxCandlestick>> result)
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
			return lastCandles;
		}

		public async Task<Result<IEnumerable<Symbol>>> GetSymbols()
		{
			var result = await _restClient.PublicData.GetInstrumentsAsync(OkxInstrumentType.Swap);

			if (result.Success)
			{
				var symbols = result
					.Data
					.Select(x => OKxHelper.ToSymbol(x.Instrument));

				return Result.Ok(symbols);
			}

			return Result.Fail(result.Error.Message);
		}
	}
}
