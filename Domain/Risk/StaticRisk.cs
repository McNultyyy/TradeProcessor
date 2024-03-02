namespace TradeProcessor.Domain.Risk
{
	public class StaticRisk : IRisk
	{
		private readonly decimal _risk;

		public StaticRisk(decimal risk)
		{
			_risk = risk;
		}

		public decimal Result()
		{
			return _risk;
		}
	}
}
