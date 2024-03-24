using Binance.Net.Clients;
using Binance.Net.Objects.Models.Futures;

namespace ListGenerator.ListGenerators;

public class CryptoPerpSMTTradingViewListGenerator : ITradingViewListGenerator
{
	public async Task GenerateAsync()
	{
		var client = new BinanceRestClient(options =>
		{

		});

		var spotSymbolsResult = await client.SpotApi.CommonSpotClient.GetSymbolsAsync();
		var spotSymbols = spotSymbolsResult.Data
			.Select(x => x.Name)
			.Where(x => x.EndsWith("USDT"))
			.Where(x => !x.StartsWith("USDC"));

		var perpSymbolsResult = await client.UsdFuturesApi.CommonFuturesClient.GetSymbolsAsync();
		var perpSymbols = perpSymbolsResult.Data
			.Where(x => ((BinanceFuturesUsdtSymbol)x.SourceObject)?.DeliveryDate > DateTime.Now)
			.Select(x => x.Name)
			.Where(x => x.EndsWith("USDT"))
			.Where(x => !x.StartsWith("USDC"))
			.Where(spotSymbols.Contains)
			.OrderByDescending(x => x);



		var bullishSmtSymbols = new List<string>();
		var bearishSmtSymbols = new List<string>();

		var timeUnit = TimeSpan.FromDays(1);
		var now = DateTime.Today - timeUnit;


		// Use 1 for standard
		// otherwise set to how many days in the past you want to check
		var backtestUnits = 1;
		var backtestNow = now - backtestUnits * timeUnit;

		foreach (var dateTime in backtestNow.Range(now, timeUnit))
		{
			var previous = dateTime - timeUnit;

			//if (dateTime.DayOfWeek.IsWeekend()) continue;

			Console.WriteLine($"[From - {previous:yyyy-MM-dd}]");
			Console.WriteLine($"[To - {dateTime:yyyy-MM-dd}]");

			foreach (var symbol in perpSymbols)
			{
				var previousTwoPerpCandles = await client.UsdFuturesApi.CommonFuturesClient
					.GetKlinesAsync(symbol, timeUnit, previous, dateTime);

				var previousTwoSpotCandles = await client.SpotApi.CommonSpotClient
					.GetKlinesAsync(symbol, timeUnit, previous, dateTime);


				if (previousTwoPerpCandles.Data.Count() == 2 &&
					previousTwoSpotCandles.Data.Count() == 2 &&
					previousTwoSpotCandles.Data.First().OpenTime == previousTwoPerpCandles.Data.First().OpenTime)
				{
					var (pd1High, pd2High, pd1Low, pd2Low) = previousTwoPerpCandles.Data.ToList().GetTuple(
						x => x[0].HighPrice,
						x => x[1].HighPrice,
						x => x[0].LowPrice,
						x => x[1].LowPrice
					);

					var (sd1High, sd2High, sd1Low, sd2Low) = previousTwoSpotCandles.Data.ToList().GetTuple(
						x => x[0].HighPrice,
						x => x[1].HighPrice,
						x => x[0].LowPrice,
						x => x[1].LowPrice
					);


					// Bearish Div.
					if (sd1High - sd2High >= 0
						!=
						pd1High - pd2High >= 0)
					{

						bearishSmtSymbols.Add(symbol);
					}

					// Bullish Div,
					if (sd1Low - sd2Low >= 0
						!=
						pd1Low - pd2Low >= 0)
					{
						bullishSmtSymbols.Add(symbol);
					}
				}
			}


			foreach (var bullishSmtSymbol in bullishSmtSymbols)
			{

				Console.WriteLine($"Found Bullish Div. for {bullishSmtSymbol}");
			}

			Console.WriteLine();

			foreach (var bearishSmtSymbol in bearishSmtSymbols)
			{

				Console.WriteLine($"Found Bearish Div. for {bearishSmtSymbol}");
			}

			var allSymbols =
				new[] { "BULL" }
					.Pipe(x => x.Concat(
						bullishSmtSymbols.Except(bearishSmtSymbols)))
					.Pipe(x => x.Concat(new[] { "BEAR" }))
					.Pipe(x => x.Concat(
						bearishSmtSymbols.Except(bullishSmtSymbols)));

			var filename = $"Daily_SMT_Watchlist_{dateTime:yyyy-MM-dd}.txt";

			File.WriteAllLines(filename,
				allSymbols.Select(symbol => $"BINANCE:{symbol}"));

		}

		Console.WriteLine("Scan Complete!");

		Console.ReadLine();
	}
}
