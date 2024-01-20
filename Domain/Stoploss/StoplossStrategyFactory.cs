using System.Text.RegularExpressions;
using TradeProcessor.Domain.TechnicalAnalysis;

namespace TradeProcessor.Domain.Stoploss
{
	public class StoplossStrategyFactory
	{
		private readonly AverageTrueRangeProvider _averageTrueRangeProvider;

		private const string AtrRegex = "([0-9])atr";

		public StoplossStrategyFactory(AverageTrueRangeProvider averageTrueRangeProvider)
		{
			_averageTrueRangeProvider = averageTrueRangeProvider;
		}

		public async Task<IStoploss> GetStoploss(Symbol symbol, BiasType bias, string? stoploss, decimal entryPrice, TimeSpan timeSpan)
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

			if (stoploss.Contains("atr", StringComparison.InvariantCultureIgnoreCase))
			{
				var atrMultiplier = int.Parse(Regex.Match(stoploss, AtrRegex).Groups[1].Value);

				var atr = await _averageTrueRangeProvider.GetCurrentAverageTrueRange(symbol, timeSpan);

				return new RelativeStoploss(entryPrice, atrMultiplier * atr, bias == BiasType.Bullish);
			}

			return new StaticStoploss(decimal.Parse(stoploss));
		}
	}
}
