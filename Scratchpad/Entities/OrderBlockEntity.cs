using System.Linq.Expressions;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Candles;

namespace Scratchpad.Entities
{
	public class OrderBlockEntity : ICandle, IEntity<Guid>
	{
		public Guid Id { get; set; } = new Guid();

		public string Symbol { get; set; }
		public decimal Open { get; set; }
		public decimal High { get; set; }
		public decimal Low { get; set; }
		public decimal Close { get; set; }
		public DateTime OpenDateTime { get; set; }
		public DateTime CloseDateTime { get; set; }

		public decimal LiquidityWick { get; set; }

		public BiasType Direction
		{
			get => this.IsBullishCandle()
				? BiasType.Bearish
				: BiasType.Bullish;
			set
			{
				// do nothing
			}
		}
	}
}
