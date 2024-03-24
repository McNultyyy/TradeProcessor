using Binance.Net.Clients;
using OKX.Api;
using OKX.Api.Enums;
using TradeProcessor.Domain;

namespace ListGenerator.ListGenerators;

public class BinanceBTCSpotPairsWithOkxPerpSymbolsListGenerator : ISymbolsListGenerator
{
	public async Task<IEnumerable<Symbol>> GenerateAsync()
	{
		var okxClient = new OKXRestApiClient();
		var binanceClient = new BinanceRestClient();

		var perpSymbolsResult = await okxClient.PublicData.GetUnderlyingAsync(OkxInstrumentType.Swap);
		var perpSymbols = perpSymbolsResult.Data
			.Where(x => x.EndsWith("USDT"))
			.Where(x => !x.StartsWith("USDC"))
			.Select(x => Symbol.Create(x.Replace("-", "")).Value)
			.OrderByDescending(x => x);

		var spotSymbolsResult = await binanceClient.SpotApi.CommonSpotClient.GetSymbolsAsync();
		var spotSymbols = spotSymbolsResult.Data
			.Select(x => x.Name)
			.Where(x => x.EndsWith("BTC") && x.Length > "BTC".Length) // we want to filter out "non-BTC" pairs and "BTC"
			.Select(x => Symbol.Create(x).Value)
			.Where(x => perpSymbols.Select(y => y.Base).Contains(x.Quote))
			.Select(x => x.ToString());

		var allSymbols = spotSymbols
			.Select(symbol => Symbol.Create(symbol, "BINANCE", SymbolType.Spot).Value)
			.OrderByDescending(x => x, MarketCapComparer.Instance);

		return allSymbols;
	}
}
