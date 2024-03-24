using Binance.Net.Clients;
using TradeProcessor.Domain;

namespace ListGenerator.ListGenerators
{
	public class BinanceSpotPairsOnChainListGenerator : ISymbolsListGenerator
	{
		public async Task<IEnumerable<Symbol>> GenerateAsync()
		{
			var binanceClient = new BinanceRestClient();

			var spotSymbolsResult = (await binanceClient.SpotApi.CommonSpotClient.GetSymbolsAsync()).Data.First();

			return null;
		}
	}
}
