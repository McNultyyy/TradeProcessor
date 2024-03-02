using TradeProcessor.Domain.Exchange;

namespace TradeProcessor.Domain.Risk
{
	public class RiskStrategyFactory
	{
		private readonly IExchangeRestClient _exchangeRestClient;

		public RiskStrategyFactory(IExchangeRestClient exchangeRestClient)
		{
			_exchangeRestClient = exchangeRestClient;
		}

		public async Task<IRisk> GetRisk(string riskString)
		{
			if (riskString.Contains('%'))
			{
				var riskPercentage = decimal.Parse(riskString.Replace("%", ""));

				var accountSize = (await _exchangeRestClient.GetAccountBalance()).Value;

				return new PercentageOfAccountRisk(accountSize, riskPercentage);
			}

			var riskDecimal = decimal.Parse(riskString);

			return new StaticRisk(riskDecimal);
		}
	}
}
