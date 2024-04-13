using Binance.Net.Clients;
using OKX.Api;
using OKX.Api.Enums;
using TradeProcessor.Domain;

namespace ListGenerator.ListGenerators;

public class OKxBTCSpotPairsListGenerator : ISymbolsListGenerator
{
	public async Task<IEnumerable<Symbol>> GenerateAsync()
	{
		var okxClient = new OKXRestApiClient();

		var spotSymbolsResult = await okxClient.OrderBookTrading.MarketData.GetTickersAsync(OkxInstrumentType.Spot);
		var spotSymbols = spotSymbolsResult.Data
			.Select(x => x.Instrument)
			.Where(x => x.EndsWith("BTC") && x.Length > "BTC".Length) // we want to filter out "non-BTC" pairs and "BTC"
			.Select(x => Symbol.Create(x.Replace("-", "")).Value)
			.Select(x => x.ToString());

		var allSymbols = spotSymbols
			.Select(symbol => Symbol.Create(symbol, "OKX", SymbolType.Spot).Value)
			.OrderByDescending(x => x, MarketCapComparer.Instance);

		return allSymbols;
	}
}
