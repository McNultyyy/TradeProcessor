using Bybit.Net.Clients;
using Bybit.Net.Enums;
using CryptoExchange.Net.Authentication;
using Hangfire.Server;
using System.ComponentModel;
using Bybit.Net.Interfaces.Clients;
using TradeProcessor.Api.Contracts;
using TradeProcessor.Api.Domain;
using TradeProcessor.Api.Domain.Stoploss;
using TradeProcessor.Api.Domain.TakeProfit;
using TradeProcessor.Api.Logging;
using TradeProcessor.Api.Domain.Candles;

namespace TradeProcessor.Api.FvgChaser;

public class FvgChaser
{
	private readonly IBybitSocketClient _socketClient;
	private readonly IBybitRestClient _restclient;

	private readonly ILogger<FvgChaser> _logger;

	private PerformContextLogger<FvgChaser> _performContextLogger;

	public FvgChaser(ILogger<FvgChaser> logger, IBybitSocketClient socketClient, IBybitRestClient restClient)
	{
		_logger = logger;

		_socketClient = socketClient;
		_restclient = restClient;
	}

	// {Bias} {Symbol} {Interval}
	[DisplayName("{6} {0} {1}")]
	public async Task DoWork(string symbol,
		string interval,
		decimal riskPerTrade,
		string stoploss,
		string? takeProfit,
		BiasType bias,
		PerformContext context)
	{
		_performContextLogger = new PerformContextLogger<FvgChaser>(context, _logger);

		await DoWork(
			new FvgChaserRequest(
				symbol, interval, riskPerTrade, stoploss, takeProfit, bias));
	}

	private async Task DoWork(FvgChaserRequest request)
	{
		_logger.LogInformation("Received request: {request}", request);

		Candle current = null, previous = null, previousPrevious = null;
		int candleCount = 0;

		var interval = MapToKlineInterval(request.Interval);

		var symbol = request.Symbol;

		symbol = CleanSymbol(symbol);

		request = request with { Symbol = symbol };

		/* TODO: Come up with a better way of tracking the existing WS connections, and reconnect if possible??
		// await _socketClient.DerivativesApi.UnsubscribeAllAsync();

		*/
		var result = await _socketClient
				.DerivativesApi
				.SubscribeToKlineUpdatesAsync(StreamDerivativesCategory.USDTPerp, request.Symbol, interval,
					async updates =>
					{
						var data = updates.Data.First();

						var candle = new Candle(data.OpenPrice, data.HighPrice, data.LowPrice, data.ClosePrice);

						if (data.Confirm)
						{
							if (candleCount != 3)
							{
								// Initialise the first 3 candles

								//third pass
								if (previousPrevious is null && candleCount == 2)
								{
									previousPrevious = previous with { };
									previous = current with { };
									current = candle with { };
									candleCount = 3;

									_performContextLogger.LogInformation("Loaded all 3 candles...");
									_performContextLogger.LogInformation("FVG strategy started");
								}

								// second pass
								if (previous is null && candleCount == 1)
								{
									previous = current with { };
									current = candle with { };
									candleCount = 2;


									_performContextLogger.LogInformation("Loaded second candle...");
								}

								// first pass
								if (current is null)
								{
									current = candle;
									candleCount = 1;

									_performContextLogger.LogInformation("Loaded first candle...");
								}
							}
							else
							{
								previousPrevious = previous with { };
								previous = current with { };
								current = candle with { };
							}

							if (previousPrevious is not null &&
								previous is not null &&
								current is not null)
							{
								var threeCandles = new ThreeCandles(previousPrevious, previous, current);
								if (threeCandles.TryFindImbalance(out var imbalance))
								{
									_performContextLogger.LogInformation($"Found imbalance: [{imbalance}]");

									if (imbalance.BiasType == request.Bias)
									{
										var limitPrice = imbalance.BiasType == BiasType.Bullish
											? imbalance.High
											: imbalance.Low;

										await PlaceLimitOrder(request, limitPrice);
									}
									else
									{
										_performContextLogger.LogInformation("Imbalance is not in the direction of bias - Ignored.");
									}
								}
							}
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

	private string CleanSymbol(string symbol)
	{
		if (symbol.Contains(".P"))
		{
			symbol = symbol.Replace(".P", string.Empty);
			_performContextLogger.LogInformation("Removed .P from the symbol.");
		}

		if (symbol.Contains("BYBIT:"))
		{
			symbol = symbol.Replace("BYBIT:", String.Empty);
			_performContextLogger.LogInformation("Removed BYBIT: from the symbol");
		}

		return symbol;
	}

	private KlineInterval MapToKlineInterval(string requestInterval)
	{
		if (requestInterval.Contains("m"))
		{
			requestInterval = requestInterval.Replace("m", "");
			var integer = Int32.Parse(requestInterval);

			var integerInSeconds = integer * 60;

			return (KlineInterval)integerInSeconds;
		}

		if (requestInterval.Contains("H", StringComparison.InvariantCultureIgnoreCase))
		{
			requestInterval = requestInterval.Replace("H", "");
			var integer = Int32.Parse(requestInterval);

			var integerInSeconds = integer * 60 * 60;

			return (KlineInterval)integerInSeconds;
		}

		if (requestInterval.Contains("D"))
		{
			requestInterval = requestInterval.Replace("D", "");
			var integer = Int32.Parse(requestInterval);

			var integerInSeconds = integer * 60 * 60 * 24;

			return (KlineInterval)integerInSeconds;
		}

		throw new ArgumentException($"Cannot parse {requestInterval}", nameof(requestInterval));
	}

	async Task PlaceLimitOrder(FvgChaserRequest request, decimal limitPrice)
	{
		_performContextLogger.LogInformation("Setting limit order at: {limitPrice}", limitPrice);

		var stoplossStrategy = GetStoploss(limitPrice, request);
		_performContextLogger.LogInformation("Using StoplossStrategy: {stopLossStrategy}", stoplossStrategy?.GetType().ToString());
		var stoploss = stoplossStrategy?.Result();

		var takeProfitStrategy = GetTakeProfit(limitPrice, request);
		_performContextLogger.LogInformation("Using TakeProfitStrategy: {takeProfitStrategy}", takeProfitStrategy?.GetType().ToString());
		var takeProfit = takeProfitStrategy?.Result() ?? null;

		var quantity =
			Math.Round(
				request.RiskPerTrade / Math.Abs(limitPrice - stoploss.Value),
				3);

		var orderResult = await _restclient.DerivativesApi.ContractApi.Trading.PlaceOrderAsync(
			request.Symbol,
			request.Bias == BiasType.Bullish ? OrderSide.Buy : OrderSide.Sell,
			OrderType.Limit,
			quantity,
			TimeInForce.GoodTillCanceled,
			price: limitPrice,
			positionMode: PositionMode.OneWay, //request.Bias == BiasType.Bullish ? PositionMode.OneWay : PositionMode.BothSideSell,
			takeProfitPrice: takeProfit
		);

		if (!orderResult.Success)
		{

			_performContextLogger.LogError("Order failed with {error}", orderResult.Error?.Message);
		}
		else
		{
			_performContextLogger.LogInformation("Order submitted successfully");
		}
	}


	private ITakeProfit? GetTakeProfit(decimal entryPrice, FvgChaserRequest request)
	{
		// TODO: just pass in the stoploss rather than calculate it again

		var stoplossStrategy = GetStoploss(entryPrice, request);
		var stoploss = (decimal)stoplossStrategy?.Result();

		//////////////

		var takeProfit = request.TakeProfit;

		if (takeProfit is null)
			return null;

		if (takeProfit.Contains("%"))
		{
			var tpString = takeProfit
				.Replace("+", "")
				.Replace("-", "")
				.Replace("%", "");

			return new PercentageTakeProfit(decimal.Parse(tpString), entryPrice, request.Bias == BiasType.Bullish);
		}

		if (takeProfit.Contains("+") || takeProfit.Contains("-"))
		{
			var tpString = takeProfit
				.Replace("+", "")
				.Replace("-", "");

			return new RelativeTakeProfit(entryPrice, decimal.Parse(tpString), request.Bias == BiasType.Bullish);
		}

		if (takeProfit.Contains("R", StringComparison.InvariantCultureIgnoreCase))
		{
			var tpString = takeProfit
				.Replace("R", "");

			return new RiskRewardTakeProfit(entryPrice, stoploss, decimal.Parse(tpString), request.Bias == BiasType.Bullish);
		}

		return new StaticTakeProfit(decimal.Parse(takeProfit));
	}

	private IStoploss? GetStoploss(decimal entryPrice, FvgChaserRequest request)
	{
		var stoploss = request.Stoploss;

		if (stoploss is null)
			return null;

		string? tpString;
		if (stoploss.Contains("%"))
		{
			tpString = stoploss
				.Replace("%", "")
				.Replace("+", "")
				.Replace("-", "");

			return new PercentageStoploss(decimal.Parse(tpString), entryPrice, request.Bias == BiasType.Bullish);
		}

		if (stoploss.Contains("+") || stoploss.Contains("-"))
		{
			tpString = stoploss
				.Replace("+", "")
				.Replace("-", "");

			return new RelativeStoploss(entryPrice, decimal.Parse(tpString), request.Bias == BiasType.Bullish);
		}

		return new StaticStoploss(decimal.Parse(stoploss));
	}
}
