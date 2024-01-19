﻿using System.ComponentModel;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.Exchange;
using TradeProcessor.Domain.Helpers;
using TradeProcessor.Domain.Stoploss;
using TradeProcessor.Domain.TakeProfit;

namespace TradeProcessor.Domain.Services;

public class FvgChaser : IDisposable
{
	private readonly IExchangeRestClient _exchangeRestClient;
	private readonly IExchangeSocketClient _exchangeSocketClient;
	private readonly StoplossStrategyFactory _stoplossStrategyFactory;

	private ILogger<FvgChaser> Logger { get; set; }

	public FvgChaser(ILogger<FvgChaser> logger, IExchangeRestClient exchangeRestClient, IExchangeSocketClient exchangeSocketClient, StoplossStrategyFactory stoplossStrategyFactory)
	{
		Logger = logger;
		_exchangeRestClient = exchangeRestClient;
		_exchangeSocketClient = exchangeSocketClient;
		_stoplossStrategyFactory = stoplossStrategyFactory;
	}

	[DisplayName("{5} {0} {1}")] // Used by Hangfire console for JobName
	public async Task DoWork(Symbol symbol,
		string interval,
		decimal riskPerTrade,
		string stoploss,
		string? takeProfit,
		BiasType bias,
		IEnumerable<GapType> gapTypes)
	{
		await _exchangeRestClient.EnsureMaxCrossLeverage(symbol);


		int candleCount = 0;

		var intervalTimeSpan = TimeHelper.IntervalStringToTimeSpan(interval);

		var now = DateTime.UtcNow;

		// todo: review why we need 4 previous - could be because the current candle is forming?
		var dateFromFourTimeUnitsAgo = now - (4 * (intervalTimeSpan.TotalSeconds) * TimeSpan.FromSeconds(1));

		var lastCandles = (await _exchangeRestClient.GetCandles(
			symbol,
			intervalTimeSpan,
			dateFromFourTimeUnitsAgo,
			now))
			.Value
			.ToList();

		var (previousPrevious, previous, current) = (lastCandles[^1], lastCandles[^2], lastCandles[^3]);

		await _exchangeSocketClient.Subscribe(symbol, intervalTimeSpan,
			async candle =>
			{
				{
					previousPrevious = previous with { };
					previous = current with { };
					current = candle with { };

					var threeCandles = new ThreeCandles(previousPrevious, previous, current);
					if (threeCandles.TryFindImbalances(out var imbalances))
					{

						Logger.LogInformation("Found {imbalanceCount} imbalances.",
							imbalances.Count());

						foreach (var foundImbalance in imbalances)
						{
							Logger.LogInformation("Found imbalance: {imbalance}.",
								foundImbalance);
						}

						/*
						 TODO: only focus on the price imbalances for now.
						 later on we will prioritize which imbalances we should enter on
						*/

						var prioritisedImbalances = imbalances.OrderByGapType();
						var imbalance = prioritisedImbalances.FirstOrDefault(x => gapTypes.Contains(x.GapType));
						if (imbalance is not null)
						{
							if (imbalance.BiasType == bias)
							{
								var limitPrice = imbalance.BiasType == BiasType.Bullish
									? imbalance.High
									: imbalance.Low;

								await PlaceLimitOrder(symbol, bias, takeProfit, stoploss, limitPrice, riskPerTrade, intervalTimeSpan);
							}
							else
							{
								Logger.LogInformation(
									"Imbalance is not in the direction of bias - Ignored.");
							}
						}
						else
						{
							Logger.LogInformation(
								$"Ignoring non-{nameof(GapType.Price)} imbalance.");
						}
					}
				}
			});
	}

	async Task PlaceLimitOrder(Symbol symbol, BiasType biasType, string? takeProfit, string? stoploss, decimal limitPrice, decimal riskPerTrade, TimeSpan intervalTimeSpan)
	{
		Logger.LogInformation("Setting limit order at: {limitPrice}", limitPrice);

		var stoplossStrategy = await _stoplossStrategyFactory.GetStoploss(symbol, biasType, stoploss, limitPrice, intervalTimeSpan);
		Logger.LogInformation("Using StoplossStrategy: {stopLossStrategy}", stoplossStrategy?.GetType().ToString());
		var stoplossDecimal = stoplossStrategy?.Result();

		var takeProfitStrategy = GetTakeProfit(symbol, limitPrice, biasType, takeProfit, stoplossStrategy);
		Logger.LogInformation("Using TakeProfitStrategy: {takeProfitStrategy}", takeProfitStrategy?.GetType().ToString());
		var takeProfitDecimal = takeProfitStrategy?.Result() ?? null;

		var quantity =
			Math.Round(
				riskPerTrade / Math.Abs(limitPrice - stoplossDecimal.Value),
				3); //todo: why is this 3?

		var orderResult = await _exchangeRestClient.PlaceOrder(
			symbol,
			biasType,
			quantity,
			limitPrice,
			takeProfitDecimal
		);

		if (orderResult.IsFailed)
		{
			Logger.LogError("Order failed with {error}", orderResult.Errors.First().Message);
		}
		else
		{
			Logger.LogInformation("Order submitted successfully");
		}
	}


	private ITakeProfit? GetTakeProfit(Symbol symbol, decimal entryPrice, BiasType bias, string takeProfit, IStoploss? stoplossStrategy)
	{
		var stoplossDecimal = (decimal)stoplossStrategy?.Result();

		//////////////

		if (takeProfit is null)
			return null;

		if (takeProfit.Contains("%"))
		{
			var tpString = takeProfit
				.Replace("+", "")
				.Replace("-", "")
				.Replace("%", "");

			return new PercentageTakeProfit(decimal.Parse(tpString), entryPrice, bias == BiasType.Bullish);
		}

		if (takeProfit.Contains("+") || takeProfit.Contains("-"))
		{
			var tpString = takeProfit
				.Replace("+", "")
				.Replace("-", "");

			return new RelativeTakeProfit(entryPrice, decimal.Parse(tpString), bias == BiasType.Bullish);
		}

		if (takeProfit.Contains("R", StringComparison.InvariantCultureIgnoreCase))
		{
			var tpString = takeProfit
				.Replace("R", "");

			return new RiskRewardTakeProfit(entryPrice, stoplossDecimal, decimal.Parse(tpString), bias == BiasType.Bullish);
		}

		return new StaticTakeProfit(decimal.Parse(takeProfit));
	}

	public void Dispose()
	{
		_exchangeRestClient.Dispose();
		_exchangeSocketClient.Dispose();
	}
}
