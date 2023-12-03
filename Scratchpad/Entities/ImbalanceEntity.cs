using TradeProcessor.Domain;

namespace Scratchpad.Entities
{
	public class ImbalanceEntity : IImbalance, IEntity<Guid>
	{
		public Guid Id { get; set; }

		/*
		public string Id
		{
			get => $"{Symbol}_{BiasType}_{GapType}_{High}/{Low}";
			set { }
		}
		*/

		public string Symbol { get; set; }

		public decimal High { get; set; }
		public decimal Low { get; set; }
		public BiasType BiasType { get; set; }
		public GapType GapType { get; set; }

	}
}
