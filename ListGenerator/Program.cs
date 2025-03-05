// See https://aka.ms/new-console-template for more information

using CoinGecko.Net.Clients;
using ListGenerator.FileGenerators;
using ListGenerator.ListGenerators;

Console.WriteLine("Starting ...");

/*
var smtGenerator = new CryptoPerpSMTTradingViewListGenerator();
await smtGenerator.GenerateAsync();

var fundingRateGenerator = new CryptoFundingRateTradingViewListGenerator();
await fundingRateGenerator.GenerateAsync();

System.Environment.Exit(0);
*/

var tradingViewListOutputPath = "C:\\Users\\willi\\Projects\\TradeProcessor\\ListGenerator\\TradingViewLists";
var tradingViewAlertsListOutputPath =
	"C:\\Users\\willi\\Projects\\TradeProcessor\\ListGenerator\\TradingViewAlertsLists";

var symbolsListGenerators = new ISymbolsListGenerator[]
{
	//new BinanceSpotWithOkxPerpSymbolsListGenerator(),
	//new OKxBTCSpotPairsListGenerator(),
	//new OkxPerpCategoriesTradingViewListGenerator(),
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
