using TradeProcessor.Domain.Candles;

namespace Scratchpad.Entities
{
	public class CandleEntity : ICandle, IEntity<string>
	{
		public CandleEntity()
		{
			/*
			 * The ID currently assumes all candles are daily.
			 * todo: make this more generic
			 */

		}

		public string Id
		{
			get => Id = $"{Symbol}_{OpenDateTime:yyMMdd}";
			set { }
		}

		public string Symbol { get; set; }

		public decimal Open { get; set; }
		public decimal High { get; set; }
		public decimal Low { get; set; }
		public decimal Close { get; set; }
		public DateTime OpenDateTime { get; set; }
		public DateTime CloseDateTime { get; set; }
	}
}
