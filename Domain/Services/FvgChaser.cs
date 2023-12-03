using System.ComponentModel;
using Microsoft.Extensions.Logging;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.Exchange;
using TradeProcessor.Domain.Helpers;
using TradeProcessor.Domain.Stoploss;
using TradeProcessor.Domain.TakeProfit;

namespace TradeProcessor.Domain.Services;

public class FvgChaser
{
	private IExchangeRestClient _exchangeRestClient;
	private IExchangeSocketClient _exchangeSocketClient;

	private readonly ILogger<FvgChaser> _logger;

	public FvgChaser(ILogger<FvgChaser> logger, IExchangeRestClient exchangeRestClient, IExchangeSocketClient exchangeSocketClient)
	{
		_logger = logger;
		_exchangeRestClient = exchangeRestClient;
		_exchangeSocketClient = exchangeSocketClient;
	}

	public async Task DoWork(string symbol,
		string interval,
		decimal riskPerTrade,
		string stoploss,
		string? takeProfit,
		BiasType bias)
	{
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

						_logger.LogInformation("Found {imbalanceCount} imbalances.",
							imbalances.Count());

						foreach (var foundImbalance in imbalances)
						{
							_logger.LogInformation("Found imbalance: {imbalance}.",
								foundImbalance);
						}

						/*
						 TODO: only focus on the price imbalances for now.
						 later on we will prioritize which imbalances we should enter on
						*/
						var imbalance = imbalances.FirstOrDefault(x => x.GapType == GapType.Price);
						if (imbalance is not null)
						{
							if (imbalance.BiasType == bias)
							{
								var limitPrice = imbalance.BiasType == BiasType.Bullish
									? imbalance.High
									: imbalance.Low;

								await PlaceLimitOrder(symbol, bias, takeProfit, stoploss, limitPrice, riskPerTrade);
							}
							else
							{
								_logger.LogInformation(
									"Imbalance is not in the direction of bias - Ignored.");
							}
						}
						else
						{
							_logger.LogInformation(
								$"Ignoring non-{nameof(GapType.Price)} imbalance.");
						}
					}
				}
			});
	}

	async Task PlaceLimitOrder(string symbol, BiasType biasType, string takeProfit, string stoploss, decimal limitPrice, decimal riskPerTrade)
	{
		_logger.LogInformation("Setting limit order at: {limitPrice}", limitPrice);

		var stoplossStrategy = GetStoploss(limitPrice, biasType, stoploss);
		_logger.LogInformation("Using StoplossStrategy: {stopLossStrategy}", stoplossStrategy?.GetType().ToString());
		var stoplossDecimal = stoplossStrategy?.Result();

		var takeProfitStrategy = GetTakeProfit(limitPrice, biasType, takeProfit, stoploss);
		_logger.LogInformation("Using TakeProfitStrategy: {takeProfitStrategy}", takeProfitStrategy?.GetType().ToString());
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
			_logger.LogError("Order failed with {error}", orderResult.Errors.First().Message);
		}
		else
		{
			_logger.LogInformation("Order submitted successfully");
		}
	}


	private ITakeProfit? GetTakeProfit(decimal entryPrice, BiasType bias, string takeProfit, string stoploss)
	{
		// TODO: just pass in the stoploss rather than calculate it again

		var stoplossStrategy = GetStoploss(entryPrice, bias, stoploss);
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

	private IStoploss? GetStoploss(decimal entryPrice, BiasType bias, string stoploss)
	{
		string? tpString;
		if (stoploss.Contains("%"))
		{
			tpString = stoploss
				.Replace("%", "")
				.Replace("+", "")
				.Replace("-", "");

			return new PercentageStoploss(decimal.Parse(tpString), entryPrice, bias == BiasType.Bullish);
		}

		if (stoploss.Contains("+") || stoploss.Contains("-"))
		{
			tpString = stoploss
				.Replace("+", "")
				.Replace("-", "");

			return new RelativeStoploss(entryPrice, decimal.Parse(tpString), bias == BiasType.Bullish);
		}

		return new StaticStoploss(decimal.Parse(stoploss));
	}
}
