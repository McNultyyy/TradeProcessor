namespace TradeProcessor.Domain.Risk
{
	public class PercentageOfAccountRisk : IRisk
	{
		private readonly decimal _accountSize;
		private readonly decimal _riskPercentage;

		public PercentageOfAccountRisk(decimal accountSize, decimal riskPercentage)
		{
			_accountSize = accountSize;
			_riskPercentage = riskPercentage;
		}

		public decimal Result()
		{
			var result = _accountSize * (_riskPercentage / 100);
			return result;
		}
	}
}