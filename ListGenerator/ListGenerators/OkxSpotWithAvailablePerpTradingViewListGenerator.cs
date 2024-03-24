using OKX.Api;
using OKX.Api.Enums;
using TradeProcessor.Domain;

namespace ListGenerator.ListGenerators;

public class OkxSpotWithAvailablePerpTradingViewListGenerator : ISymbolsListGenerator
{
	public async Task<IEnumerable<Symbol>> GenerateAsync()
	{
		var client = new OKXRestApiClient();

		var spotSymbolsResult = await client.PublicData.GetInstrumentsAsync(OkxInstrumentType.Spot);
		var spotSymbols = spotSymbolsResult.Data
			.Select(x => x.Instrument)
			.Where(x => x.EndsWith("USDT"))
			.Where(x => !x.StartsWith("USDC"));

		var perpSymbolsResult = await client.PublicData.GetUnderlyingAsync(OkxInstrumentType.Swap);
		var perpSymbols = perpSymbolsResult.Data
			.Where(x => x.EndsWith("USDT"))
			.Where(x => !x.StartsWith("USDC"))
			.Where(spotSymbols.Contains)
			.OrderByDescending(x => x);

		var allSymbols = perpSymbols;
		
		return allSymbols.Select(x => Symbol.Create(x.Replace("-", ""), "OKX", SymbolType.Spot).Value);
	}
}
