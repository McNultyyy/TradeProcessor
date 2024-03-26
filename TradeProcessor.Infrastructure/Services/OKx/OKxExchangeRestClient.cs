using ApiSharp.Extensions;
using ApiSharp.Models;
using FluentResults;
using Microsoft.Extensions.Logging;
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

				_logger.LogInformation("Quantity has been rounded down to {newQuantity}",
					newQuantity);
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

			// todo: add take profits

			if (orderResult.Success)
				return Result.Ok();

			return Result.Fail(orderResult.Error.Message);
		}

		public async Task<Result<IEnumerable<Candle>>> GetCandles(Symbol symbol, TimeSpan interval, DateTime from,
			DateTime to)
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

		private IEnumerable<decimal> AvailableLeverages()
		{
			return new[] {125m, 100, 75m, 50m, 30m, 20m, 10m, 5m, 3m, 2m, 1m};
		}
	}
}
