using ApiSharp.Extensions;
using ApiSharp.Models;
using FluentResults;
using Microsoft.Extensions.Logging;
using MoreLinq;
using OKX.Api;
using OKX.Api.Enums;
using OKX.Api.Models.MarketData;
using OKX.Api.Models.Trade;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.Exchange;

namespace TradeProcessor.Infrastructure.Services.OKx
{
	public class OKxExchangeRestClient : IExchangeRestClient
	{
		private readonly OKXRestApiClient _restClient;
		private readonly ILogger<OKxExchangeRestClient> _logger;

		public OKxExchangeRestClient(OKXRestApiClient restClient, ILogger<OKxExchangeRestClient> logger)
		{
			_restClient = restClient;
			_logger = logger;
		}

		async Task<Result> IExchangeRestClient.PlaceOrder(Symbol symbol, BiasType bias, decimal quantity,
			decimal limitPrice,
			bool setStoploss,
			decimal? takeProfit, decimal? stopLoss = null)
		{
			_logger.LogInformation("Placing {orderSide} limit for {symbol} at price {limitPrice}",
				bias is BiasType.Bullish ? "Buy" : "Sell",
				symbol,
				limitPrice);

			var okxSymbol = OKxHelper.ToOkxSymbol(symbol);

			var result =
				await _restClient.PublicData.GetInstrumentsAsync(OkxInstrumentType.Swap, instrumentId: okxSymbol);
			var contractValue = result.Data.First().ContractValue.Value;

			if (quantity % contractValue != 0)
			{
				_logger.LogInformation("Quantity {quantity} must be a multiple of {contractValue}",
					quantity, contractValue);

				var newQuantity = quantity.RoundDownToMultiple(contractValue);
				quantity = newQuantity / contractValue;

				if (quantity == 0)
				{
					_logger.LogWarning("Quantity has been rounded to ZERO. Unable to place trade.");
					return Result.Fail(
						"Unable to create an order with the requested quantity, given the current risk parameters");
				}
				else
				{
					_logger.LogInformation("Quantity has been rounded down to {newQuantity}",
						newQuantity);
				}
			}

			decimal? tpOrderTriggerPrice = null;
			if (takeProfit is not null)
			{
				/*
				 * OKx has an awkward API and doesn't allow us to set the TP at the exact time of creating the trade.
				 * So when price reaches 30% of the way from our entry to the profit target,
				 * we should place our limit order.
				 */

				var diff = Math.Abs(takeProfit.Value - limitPrice);
				var percentageOfDiff = diff * 0.30m;

				tpOrderTriggerPrice = bias == BiasType.Bullish
					? limitPrice + percentageOfDiff
					: limitPrice - percentageOfDiff;
				_logger.LogInformation("Set take profit trigger price to {takeProfitTriggerPrice}",
					tpOrderTriggerPrice);
			}

			decimal? slOrderTriggerPrice = null;
			if (stopLoss is not null && setStoploss)
			{
				/*
				 * Same as above
				 */

				var diff = Math.Abs(stopLoss.Value - limitPrice);
				var percentageOfDiff = diff * 0.30m;

				slOrderTriggerPrice = bias == BiasType.Bullish
					? limitPrice - percentageOfDiff
					: limitPrice + percentageOfDiff;
				_logger.LogInformation("Set stop loss trigger price to {stopLossTriggerPrice}", slOrderTriggerPrice);
			}

			var orderResult = await _restClient.OrderBookTrading.Trade.PlaceOrderAsync(
				instrumentId: okxSymbol,
				tradeMode: OkxTradeMode.Cross,
				orderSide: bias == BiasType.Bullish ? OkxOrderSide.Buy : OkxOrderSide.Sell,
				positionSide: OkxPositionSide.Net,
				orderType: OkxOrderType.LimitOrder,
				quantityType: OkxQuantityType.BaseCurrency,
				size: quantity,
				price: limitPrice,
				tpOrdPx: takeProfit,
				tpTriggerPx: tpOrderTriggerPrice,
				slOrdPx: setStoploss ? stopLoss : null,
				slTriggerPx: setStoploss ? slOrderTriggerPrice : null);

			if (orderResult.Success)
			{
				_logger.LogInformation("Successfully created limit order.");
				return Result.Ok();
			}

			var failureMessage = orderResult.Error.Message;
			_logger.LogError(failureMessage);
			return Result.Fail(failureMessage);
		}

		public async Task<Result<IEnumerable<Candle>>> GetCandles(Symbol symbol, TimeSpan interval, DateTime from,
			DateTime to)
		{
			_logger.LogInformation("Getting {interval} candles for {symbol} from {from} to {to}",
				interval, symbol, from, to);

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

		private static IEnumerable<Candle> ConvertToCandles(Symbol symbol, TimeSpan interval,
			RestCallResult<IEnumerable<OkxCandlestick>> result)
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

		public async Task<Result> EnsureMaxCrossLeverage(Symbol symbol)
		{
			var okxSymbol = OKxHelper.ToOkxSymbol(symbol);

			var result =
				await _restClient.PublicData.GetInstrumentsAsync(OkxInstrumentType.Swap, instrumentId: okxSymbol);
			if (!result.Success)
			{
				return Result.Fail(result.Error.Message);
			}

			var maxLeverage = result.Data.First().MaximumLeverage.Value;

			_logger.LogInformation("Attempting to set {symbol} leverage to {leverage}",
				symbol.ToString(), maxLeverage);

			var setLeverageResult = await _restClient.TradingAccount.SetAccountLeverageAsync(
				maxLeverage,
				marginMode: OkxMarginMode.Cross,
				instrumentId: okxSymbol);

			if (setLeverageResult.Success)
			{
				_logger.LogInformation("Successfully set leverage");
				return Result.Ok();
			}

			return Result.Fail("Could not set leverage");
		}

		public async Task<Result<decimal>> GetAccountBalance()
		{
			var result = await _restClient.TradingAccount.GetAccountBalanceAsync();


			if (result.Success)
			{
				return Result.Ok(result.Data.TotalEquity);
			}

			return Result.Fail(result.Error.Message);
		}

		public async Task<Result> CancelAllOrders()
		{
			var getActiveOrders = await _restClient.OrderBookTrading.Trade.GetOrderListAsync(OkxInstrumentType.Swap);
			var cancelOrderRequests = getActiveOrders.Data
					.Select(x => new OkxOrderCancelRequest()
					{
						ClientOrderId = x.ClientOrderId,
						InstrumentId = x.Instrument,
						OrderId = x.OrderId
					})
					.Batch(20);

			foreach (var batch in cancelOrderRequests)
			{
				var cancelOrdersResponse =
					await _restClient.OrderBookTrading.Trade.CancelMultipleOrdersAsync(batch);

				if (!cancelOrdersResponse.Success)
					return Result.Fail(cancelOrdersResponse.Error.Message);
			}
			
			return Result.Ok();
		}
	}
}
