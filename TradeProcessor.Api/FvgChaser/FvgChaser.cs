using Hangfire.Server;
using System.ComponentModel;
using TradeProcessor.Api.Contracts;
using TradeProcessor.Api.Logging;
using TradeProcessor.Domain.Exchange;

namespace TradeProcessor.Api.FvgChaser;

/*

//todo: more to domain services
public class FvgChaser
{
	private IExchangeRestClient _exchangeRestClient;
	private IExchangeSocketClient _exchangeSocketClient;

	private readonly ILogger<FvgChaser> _logger;

	private PerformContextLogger<FvgChaser> _performContextLogger;

	public FvgChaser(ILogger<FvgChaser> logger, IExchangeRestClient exchangeRestClient, IExchangeSocketClient exchangeSocketClient)
	{
		_logger = logger;
		_exchangeRestClient = exchangeRestClient;
		_exchangeSocketClient = exchangeSocketClient;
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

		int candleCount = 0;

		var interval = TimeHelper.IntervalStringToTimeSpan(request.Interval);

		var symbol = request.Symbol;

		// todo: we should be doing this earlier
		symbol = CleanSymbol(symbol);

		request = request with { Symbol = symbol };

		var now = DateTime.UtcNow;
		var dateFromFourTimeUnitsAgo = now - (4 * (interval.TotalSeconds) * TimeSpan.FromSeconds(1));
		var lastCandles = (await _exchangeRestClient.GetCandles(
			symbol,
			TimeHelper.IntervalStringToTimeSpan(request.Interval),
			dateFromFourTimeUnitsAgo,
			now))
			.Value
			.ToList();

		var (previousPrevious, previous, current) = (lastCandles[^1], lastCandles[^2], lastCandles[^3]);

		await _exchangeSocketClient.Subscribe(request.Symbol, TimeHelper.IntervalStringToTimeSpan(request.Interval),
			async candle =>
			{
				{
					previousPrevious = previous with { };
					previous = current with { };
					current = candle with { };

					var threeCandles = new ThreeCandles(previousPrevious, previous, current);
					if (threeCandles.TryFindImbalances(out var imbalances))
					{

						_performContextLogger.LogInformation("Found {imbalanceCount} imbalances.",
							imbalances.Count());

						foreach (var foundImbalance in imbalances)
						{
							_performContextLogger.LogInformation("Found imbalance: {imbalance}.",
								foundImbalance);
						}


						var imbalance = imbalances.FirstOrDefault(x => x.GapType == GapType.Price);
						if (imbalance is not null)
						{
							if (imbalance.BiasType == request.Bias)
							{
								var limitPrice = imbalance.BiasType == BiasType.Bullish
									? imbalance.High
									: imbalance.Low;

								await PlaceLimitOrder(request, limitPrice);
							}
							else
							{
								_performContextLogger.LogInformation(
									"Imbalance is not in the direction of bias - Ignored.");
							}
						}
						else
						{
							_performContextLogger.LogInformation(
								$"Ignoring non-{nameof(GapType.Price)} imbalance.");
						}


					}
				}
			});
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
				3); //todo: why is this 3?

		var orderResult = await _exchangeRestClient.PlaceOrder(
			request.Symbol,
			request.Bias,
			quantity,
			limitPrice,
			takeProfit
		);

		if (orderResult.IsFailed)
		{
			_performContextLogger.LogError("Order failed with {error}", orderResult.Errors.First().Message);
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
*/
