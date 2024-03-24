using Binance.Net.Clients;
using OKX.Api;
using OKX.Api.Enums;
using TradeProcessor.Domain;

namespace ListGenerator.ListGenerators;

public class BinanceSpotWithOkxPerpSymbolsListGenerator : ISymbolsListGenerator
{
	public async Task<IEnumerable<Symbol>> GenerateAsync()
	{
		var okxClient = new OKXRestApiClient();
		var binanceClient = new BinanceRestClient();

		var perpSymbolsResult = await okxClient.PublicData.GetUnderlyingAsync(OkxInstrumentType.Swap);
		var perpSymbols = perpSymbolsResult.Data
			.Where(x => x.EndsWith("USDT"))
			.Where(x => !x.StartsWith("USDC"))
			.Select(x => x.Replace("-", ""))
			.OrderByDescending(x => x);

		var spotSymbolsResult = await binanceClient.SpotApi.CommonSpotClient.GetSymbolsAsync();
		var spotSymbols = spotSymbolsResult.Data
			.Select(x => x.Name)
			.Where(x => x.EndsWith("USDT"))
			.Where(x => !x.StartsWith("USDC"))
			.Where(x => perpSymbols.Contains(x));

		var allSymbols = spotSymbols
			.Select(symbol => Symbol.Create(symbol, "BINANCE", SymbolType.Spot).Value)
			.OrderByDescending(x => x, MarketCapComparer.Instance);

		return allSymbols;
	}
}
