using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TradeProcessor.Domain.TechnicalAnalysis;

namespace TradeProcessor.Domain.Stoploss
{
	public class StoplossStrategyFactory
	{
		private readonly AverageTrueRangeProvider _averageTrueRangeProvider;
		private ILogger<StoplossStrategyFactory> _logger;

		private const string AtrRegex = "([0-9])atr";

		public StoplossStrategyFactory(AverageTrueRangeProvider averageTrueRangeProvider,
			ILogger<StoplossStrategyFactory> logger)
		{
			_averageTrueRangeProvider = averageTrueRangeProvider;
			_logger = logger;
		}

		public async Task<IStoploss> GetStoploss(Symbol symbol, BiasType bias, string? stoploss, decimal entryPrice,
			TimeSpan timeSpan, (decimal low, decimal high)? fvg)
		{
			var stoplossStrategy = stoploss switch
			{
				null => throw new ArgumentNullException(nameof(stoploss)),

				_ when stoploss.Contains('%') =>
					CreatePercentageStoploss(bias, stoploss, entryPrice),

				_ when stoploss.Contains('+') || stoploss.Contains('-') =>
					CreateRelativeStoploss(bias, stoploss, entryPrice),

				_ when stoploss.Contains("atr", StringComparison.InvariantCultureIgnoreCase) =>
					await CreateAtrStoploss(symbol, bias, stoploss, entryPrice, timeSpan),

				_ when stoploss.Contains("fvg") =>
					CreateFvgStoploss(bias, fvg.Value.low, fvg.Value.high), // todo: assume its not null at this point

				_ => new StaticStoploss(decimal.Parse(stoploss))
			};

			_logger.LogInformation("Using StoplossStrategy: {stopLossStrategy}",
				stoplossStrategy?.GetType().ToString());

			return stoplossStrategy;
		}

		private async Task<IStoploss> CreateAtrStoploss(Symbol symbol, BiasType bias, string stoploss,
			decimal entryPrice,
			TimeSpan timeSpan)
		{
			var atrMultiplier = int.Parse(Regex.Match(stoploss, AtrRegex).Groups[1].Value);
			var atr = await _averageTrueRangeProvider.GetCurrentAverageTrueRange(symbol, timeSpan);

			return new RelativeStoploss(entryPrice, atrMultiplier * atr, bias == BiasType.Bullish);
		}

		private static IStoploss CreatePercentageStoploss(BiasType bias, string stoploss, decimal entryPrice)
		{
			var tpString = stoploss
				.Replace("%", "")
				.Replace("+", "")
				.Replace("-", "");

			return new PercentageStoploss(decimal.Parse(tpString), entryPrice, bias == BiasType.Bullish);
		}

		private static IStoploss CreateRelativeStoploss(BiasType bias, string stoploss, decimal entryPrice)
		{
			var offset = stoploss
				.Replace("+", "")
				.Replace("-", "");

			return new RelativeStoploss(entryPrice, decimal.Parse(offset), bias == BiasType.Bullish);
		}

		private static IStoploss CreateFvgStoploss(BiasType bias, decimal fvgLow, decimal fvgHigh)
		{
			return new FvgStoploss(fvgLow, fvgHigh, bias.IsBullish());
		}
	}
}
