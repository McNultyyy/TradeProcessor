using Microsoft.Extensions.Logging;
using TradeProcessor.Domain.Stoploss;

namespace TradeProcessor.Domain.TakeProfit
{
	public class TakeProfitStrategyFactory
	{
		private readonly ILogger<TakeProfitStrategyFactory> _logger;

		public TakeProfitStrategyFactory(ILogger<TakeProfitStrategyFactory> logger)
		{
			_logger = logger;
		}

		public ITakeProfit? GetTakeProfit(BiasType bias, string? takeProfit, decimal entryPrice,
			IStoploss? stoplossStrategy, (decimal low, decimal high) fvg)
		{
			var takeProfitStrategy = takeProfit switch
			{
				null => null,

				_ when takeProfit.Contains('%') =>
					CreatePercentageTakeProfit(bias, takeProfit, entryPrice),

				_ when takeProfit.Contains('+') || takeProfit.Contains('-') =>
					CreateRelativeTakeProfit(bias, takeProfit, entryPrice),

				_ when takeProfit.Contains("R") =>
					CreateRiskRewardTakeProfit(bias, entryPrice, stoplossStrategy.Result(), takeProfit),

				_ when takeProfit.Contains("fvg") =>
					CreateFvgTakeProfit(bias, fvg.low, fvg.high),

				_ when !String.IsNullOrEmpty(takeProfit)
					=> new StaticTakeProfit(decimal.Parse(takeProfit)),

				_ => throw new ArgumentException(nameof(takeProfit))
			};

			_logger.LogInformation("Using TakeProfitStrategy: {takeProfitStrategy}",
				takeProfitStrategy?.GetType().ToString());

			return takeProfitStrategy;
		}

		private static ITakeProfit CreatePercentageTakeProfit(BiasType bias, string takeProfit, decimal entryPrice)
		{
			var tpString = takeProfit
				.Replace("+", "")
				.Replace("-", "");

			return new RelativeTakeProfit(entryPrice, decimal.Parse(tpString), bias == BiasType.Bullish);
		}

		private static ITakeProfit CreateRelativeTakeProfit(BiasType bias, string takeProfit, decimal entryPrice)
		{
			var tpString = takeProfit
				.Replace("+", "")
				.Replace("-", "")
				.Replace("%", "");

			return new PercentageTakeProfit(decimal.Parse(tpString), entryPrice, bias == BiasType.Bullish);
		}

		private static ITakeProfit CreateRiskRewardTakeProfit(BiasType bias, decimal entryPrice, decimal stoploss,
			string takeProfit)
		{
			var riskReward = decimal.Parse(takeProfit.Replace("R", ""));

			return new RiskRewardTakeProfit(entryPrice, stoploss, riskReward, bias.IsBullish());
		}

		private static ITakeProfit CreateFvgTakeProfit(BiasType bias, decimal fvgLow, decimal fvgHigh)
		{
			return new FvgTakeProfit(fvgLow, fvgHigh, bias.IsBullish());
		}
	}
}
