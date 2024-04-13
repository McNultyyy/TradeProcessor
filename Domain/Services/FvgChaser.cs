using System.ComponentModel;
using Microsoft.Extensions.Logging;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.Exchange;
using TradeProcessor.Domain.Helpers;
using TradeProcessor.Domain.Logging;
using TradeProcessor.Domain.Risk;
using TradeProcessor.Domain.Stoploss;
using TradeProcessor.Domain.TakeProfit;

namespace TradeProcessor.Domain.Services;

public class FvgChaser
{
	private readonly IExchangeRestClient _exchangeRestClient;
	private readonly IExchangeSocketClient _exchangeSocketClient;
	private readonly TradeParser _tradeParser;
	private readonly ILogger<FvgChaser> _logger;

	public FvgChaser(
		ILogger<FvgChaser> logger,
		IExchangeRestClient exchangeRestClient,
		IExchangeSocketClient exchangeSocketClient,
		TradeParser tradeParser)
	{
		_logger = logger;
		_exchangeRestClient = exchangeRestClient;
		_exchangeSocketClient = exchangeSocketClient;
		_tradeParser = tradeParser;
	}

	[DisplayName("{6} {0} {1}")] // Used by Hangfire console for JobName
	public async Task DoWork(Symbol symbol,
		string interval,
		string riskPerTrade,
		string? stoploss,
		bool setStoploss,
		string? takeProfit,
		BiasType bias,
		int? numberOfActiveOrders,
		int? numberOfTrades,
		IEnumerable<GapType> gapTypes,
		FvgEntryType fvgEntryType,
		CancellationToken cancellationToken)
	{
		using var _ = _logger.BeginScopeWith(new {Symbol = symbol, Bias = bias});

		_logger.LogInformation("Running {biasType} {serviceName for {symbol}",
			bias, nameof(FvgChaser), symbol);

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

		var currentTradesCount = 0;

		await _exchangeSocketClient.Subscribe(symbol, intervalTimeSpan,
			async candle =>
			{
				// todo: add current trade count logic

				previousPrevious = previous with { };
				previous = current with { };
				current = candle with { };

				var threeCandles = new ThreeCandles(previousPrevious, previous, current);
				if (threeCandles.TryFindImbalances(out var imbalances))
				{
					_logger.LogInformation("Found {imbalanceCount} imbalances of type {imbalanceTypes}",
						imbalances.Count(), String.Join(",", imbalances));

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
							var limitPrice = CalculateFvgEntryPrice(imbalance, fvgEntryType);

							(decimal low, decimal high) =
								(threeCandles.PreviousPrevious.Low, threeCandles.Current.High);

							await PlaceLimitOrder(symbol, bias, takeProfit, stoploss, limitPrice, riskPerTrade,
								intervalTimeSpan, (low, high), setStoploss);

							// when numberOfTrades is null it means we want this job to run forever.
							// so we dont want to increase the currentTradeCount (which will cause the job to end)
							if (numberOfTrades is not null)
							{
								_logger.LogInformation("Incrementing current trades count");
								currentTradesCount++;
								_logger.LogInformation("Current trades count is {currentTradesCount}",
									currentTradesCount);
							}
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
							$"Ignoring imbalance.");
					}
				}
			});

		while (!cancellationToken.IsCancellationRequested)
		{
			// do nothing
		}

		_logger.LogInformation("Cancellation has been requested = {isCancellationRequested}." +
		                       "Shutting down.",
			cancellationToken.IsCancellationRequested);
	}

	private decimal CalculateFvgEntryPrice(IImbalance imbalance, FvgEntryType fvgEntryType)
	{
		_logger.LogInformation("Using entry type of {entryType}", fvgEntryType);

		return fvgEntryType switch
		{
			FvgEntryType.Premium =>
				imbalance.BiasType == BiasType.Bullish ? imbalance.High : imbalance.Low,
			FvgEntryType.Discount =>
				imbalance.BiasType == BiasType.Bullish ? imbalance.Low : imbalance.High,
			FvgEntryType.Midpoint =>
				(imbalance.High + imbalance.Low) / 2, // todo: see whether we need to round

			_ => throw new ArgumentOutOfRangeException(nameof(fvgEntryType), fvgEntryType, null)
		};
	}

	async Task PlaceLimitOrder(Symbol symbol, BiasType biasType, string? takeProfit, string? stoploss,
		decimal limitPrice, string riskPerTrade, TimeSpan intervalTimeSpan, (decimal low, decimal high) fvg,
		bool setStoploss)
	{
		_logger.LogInformation("Setting limit order at: {limitPrice}", limitPrice);

		var trade = await _tradeParser.Parse(symbol, biasType, takeProfit, stoploss, limitPrice, riskPerTrade,
			intervalTimeSpan,
			setStoploss, fvg);

		var orderResult = await _exchangeRestClient.PlaceOrder(trade);

		if (orderResult.IsFailed)
		{
			_logger.LogError("Order failed with {error}", orderResult.Errors.First().Message);
		}
		else
		{
			_logger.LogInformation("Order submitted successfully");
		}
	}
}
