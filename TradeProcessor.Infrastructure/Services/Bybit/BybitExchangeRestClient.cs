using Bybit.Net.Enums;
using Bybit.Net.Interfaces.Clients;
using FluentResults;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.Exchange;

namespace TradeProcessor.Infrastructure.Services.Bybit
{
	public class BybitExchangeRestClient : IExchangeRestClient
	{
		private readonly IBybitRestClient _restClient;

		public BybitExchangeRestClient(IBybitRestClient restClient)
		{
			_restClient = restClient;
		}

		async Task<Result> IExchangeRestClient.PlaceOrder(Symbol symbol, BiasType bias, decimal quantity, decimal price,
			bool setStoploss,
			decimal? takeProfit, decimal? stopLoss = null)
		{
			var orderResult = await _restClient.DerivativesApi.ContractApi.Trading.PlaceOrderAsync(
				BybitHelper.ToBybitSymbol(symbol),
				bias == BiasType.Bullish ? OrderSide.Buy : OrderSide.Sell,
				OrderType.Limit,
				quantity,
				TimeInForce.GoodTillCanceled,
				price: price,
				positionMode: PositionMode.OneWay, //request.Bias == BiasType.Bullish ? PositionMode.OneWay : PositionMode.BothSideSell,
				takeProfitPrice: takeProfit);

			if (orderResult.Success)
				return Result.Ok();

			return Result.Fail(orderResult.Error.Message);
		}

		public async Task<Result<IEnumerable<Candle>>> GetCandles(Symbol symbol, TimeSpan interval, DateTime from, DateTime to)
		{
			var result = await _restClient.V5Api.ExchangeData.GetKlinesAsync(
				Category.Linear,
				BybitHelper.ToBybitSymbol(symbol),
				BybitHelper.MapToKlineInterval(interval),
				from, to);

			if (result.Success)
			{
				var lastCandles = result
					.Data
					.List
					.Select(x =>
						new Candle(
							x.OpenPrice,
							x.HighPrice,
							x.LowPrice,
							x.ClosePrice,
							x.StartTime,
							x.StartTime + interval));

				return Result.Ok(lastCandles);
			}
			else
				return Result.Fail(result.Error.Message);
		}

		public async Task<Result<IEnumerable<Symbol>>> GetSymbols()
		{
			var result = await _restClient.DerivativesApi.ExchangeData.GetSymbolsAsync(Category.Linear);

			if (result.Success)
			{
				var symbols = result
					.Data
					.Data
					.Select(x => new Symbol(x.BaseCurrency, x.QuoteCurrency));

				return Result.Ok(symbols);
			}

			return Result.Fail(result.Error.Message);
		}

		public Task<Result> EnsureMaxCrossLeverage(Symbol symbol)
		{
			throw new NotImplementedException();
		}

		public Task<Result<decimal>> GetAccountBalance()
		{
			throw new NotImplementedException();
		}
	}
}
