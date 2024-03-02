using Swashbuckle.AspNetCore.Filters;
using TradeProcessor.Api.Contracts.FvgChaser;
using TradeProcessor.Domain;

namespace TradeProcessor.Api.Examples
{
	public class ExampleFvgChaserRequest : IExamplesProvider<FvgChaserRequest>
	{
		public FvgChaserRequest GetExamples()
		{
			return new FvgChaserRequest("BTCUSDT", "5m", "50", "1%", null, BiasType.Bullish);
		}
	}
}
