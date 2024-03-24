// See https://aka.ms/new-console-template for more information

using CoinGecko.Net.Clients;
using ListGenerator.FileGenerators;
using ListGenerator.ListGenerators;

Console.WriteLine("Starting ...");


var smtGenerator = new CryptoPerpSMTTradingViewListGenerator();
await smtGenerator.GenerateAsync();

throw new Exception("Stop");

var tradingViewListOutputPath = "C:\\Users\\willi\\Projects\\TradeProcessor\\ListGenerator\\TradingViewLists";
var tradingViewAlertsListOutputPath = "C:\\Users\\willi\\Projects\\TradeProcessor\\ListGenerator\\TradingViewAlertsLists";

var coinGeckoApi = new CoinGeckoRestClient();
var g = await coinGeckoApi.Api.GetAssetPlatformsAsync();

var symbolsListGenerators = new ISymbolsListGenerator[]
{
	//new BinanceSpotWithOkxPerpSymbolsListGenerator(),
	new BinanceBTCSpotPairsWithOkxPerpSymbolsListGenerator(),
	//new OkxSpotOnlyTradingViewListGenerator(),
	//new OkxSpotWithAvailablePerpTradingViewListGenerator()
};

var tradingViewAlertsInputFileGenerator = new TradingViewAlertsInputFileGenerator(tradingViewAlertsListOutputPath);
var tradingViewWatchlistFileGenerator = new TradingViewWatchlistFileGenerator(tradingViewListOutputPath);

foreach (var symbolsListGenerator in symbolsListGenerators)
{
	Console.WriteLine($"Generating files for {symbolsListGenerator.GetType().Name}");
	await tradingViewAlertsInputFileGenerator.GenerateAsync(symbolsListGenerator);
	await tradingViewWatchlistFileGenerator.GenerateAsync(symbolsListGenerator);
}

Console.WriteLine("Done");
