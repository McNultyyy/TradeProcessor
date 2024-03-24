using Binance.Net.Clients;
using Binance.Net.Objects.Models.Futures;

namespace ListGenerator.ListGenerators;

public class CryptoFundingRateTradingViewListGenerator : ITradingViewListGenerator
{
	public async Task GenerateAsync()
	{
		var client = new BinanceRestClient(options =>
		{
			options.OutputOriginalData = true;
		});

		var perpSymbolsResult = await client.UsdFuturesApi.CommonFuturesClient.GetSymbolsAsync();
		var perpSymbols = perpSymbolsResult.Data
			.Where(x => ((BinanceFuturesUsdtSymbol)x.SourceObject)?.DeliveryDate > DateTime.Now)
			.Where(x => x.Name.EndsWith("USDT"));


		var fundingRates = perpSymbols
			.ToDictionary(
				x => x.Name,
				x => client.UsdFuturesApi.ExchangeData.GetFundingRatesAsync(x.Name).Result.Data.First().FundingRate
			);

		var negativeFundingRates = fundingRates.Where(x => x.Value < 0);
		var positiveFundingRates = fundingRates.Where(x => x.Value > 0);

		var bullishSymbols = negativeFundingRates
			.OrderBy(x => x.Value)
			.Take(10)
			.Select(x => x.Key);


		var bearishSymbols = positiveFundingRates
			.OrderByDescending(x => x.Value)
			.Take(10)
			.Select(x => x.Key);

		var allSymbols =
			new[] { "BULL" }.Concat(bullishSymbols
				.Pipe(x =>
					x.Concat(new[] { "BEAR" }).Concat(bearishSymbols)));

		var dateTime = DateTime.UtcNow;
		var filename = $"Daily_FundingRate_Watchlist_{dateTime:yyyy-MM-dd}.txt";

		await File.WriteAllLinesAsync(filename,
			allSymbols.Select(symbol => $"BINANCE:{symbol}"));
	}
}
