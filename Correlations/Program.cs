using Binance.Net.Clients;
using Binance.Net.Enums;
using CryptoExchange.Net.Authentication;
using MathNet.Numerics.Statistics;

string dataDir = @"C:\Users\willi\Projects\TradeProcessor\Correlations\Data";


var client = new BinanceRestClient();

var symbolss =
	(await client.SpotApi.CommonSpotClient.GetSymbolsAsync()).Data.Select(x => x.Name)
	.Where(x => x.EndsWith("USDT") && !x.EndsWith("UPUSDT") && !x.EndsWith("DOWNUSDT"))
	.ToList();


var interval = KlineInterval.OneDay;

var to = DateTime.Now;
var from = to - TimeSpan.FromDays(180);

await Parallel.ForEachAsync(symbolss, async (symbol, _) =>
{
	if (!File.Exists(Path.Combine(dataDir, symbol + ".csv")))
	{
		var data = await client.SpotApi.ExchangeData.GetKlinesAsync(symbol, interval, from, to);

		var candleData = data.Data.Select(x => new
		{
			DateTime = x.OpenTime,
			Open = x.OpenPrice,
			High = x.HighPrice,
			Low = x.LowPrice,
			Close = x.ClosePrice
		});

		await File.WriteAllLinesAsync($"{dataDir}/{symbol}.csv", candleData.Select(x =>
			$"{x.DateTime},{x.Open},{x.High},{x.Low},{x.Close}"
		));
	}
});

// Dictionary to store the data for each asset
Dictionary<string, List<(DateTime, double)>> assetData = new Dictionary<string, List<(DateTime, double)>>();

// Read the data from CSV files
foreach (string asset in symbolss)
{
	string filePath = Path.Combine(dataDir, asset + ".csv");
	List<(DateTime, double)> data = File.ReadLines(filePath)
		.Skip(1) // skip the header row
		.Select(line => line.Split(','))
		.Select(fields => (DateTime.Parse(fields[0]), double.Parse(fields[4]))) // use the "Close" price
		.ToList();
	assetData[asset] = data;
}

// Calculate the correlation between all pairs of assets
List<(string, string, double)> correlations = new List<(string, string, double)>();
for (int i = 0; i < symbolss.Count - 1; i++)
{
	for (int j = i + 1; j < symbolss.Count; j++)
	{
		var first = assetData[symbolss[i]];
		var second = assetData[symbolss[j]];

		if (first.Count == second.Count) // skip those without the required historical data
		{
			double correlation = CalculateCorrelation(first, second);
			correlations.Add((symbolss[i], symbolss[j], correlation));
		}
	}
}


// Display the pairs with the highest correlation
var numberOfPairs = 20;
var numberOfBestCorrelations = 5;


var correlationDictionary = new Dictionary<string, List<(string, double)>>();

foreach (var correlation in correlations)
{
	if (!correlationDictionary.ContainsKey(correlation.Item1))
	{
		correlationDictionary[correlation.Item1] = new List<(string, double)>();
	}

	if (!correlationDictionary.ContainsKey(correlation.Item2))
	{
		correlationDictionary[correlation.Item2] = new List<(string, double)>();
	}

	if (correlationDictionary.ContainsKey(correlation.Item1))
	{
		correlationDictionary[correlation.Item1].Add((correlation.Item2, correlation.Item3));
	}

	if (correlationDictionary.ContainsKey(correlation.Item2))
	{
		correlationDictionary[correlation.Item2].Add((correlation.Item1, correlation.Item3));
	}
}


var bestCorrelations = correlationDictionary
	.Select(x =>
	{
		var bCor = x.Value
			.OrderByDescending(y => y.Item2)
			.Take(numberOfBestCorrelations)
			.ToList();

		/*
		var highestCorrelatedAsset = x.Value.MaxBy(y => y.Item2);

		return new
		{
		    Asset1 = x.Key,
		    Asset2 = highestCorrelatedAsset.Item1,

		    Correlation = highestCorrelatedAsset.Item2
		};
		*/
		return new KeyValuePair<string, List<(string, double)>>(x.Key, bCor);
	});


var symbolsAverageCorrelation = correlationDictionary
	.ToDictionary(
		x => x.Key,
		x =>  x.Value.Average(y => y.Item2))
	.OrderByDescending(x => 1 - Math.Abs(x.Value)); // find the correlations closest to 0

// Prints out the symbols average correlation
var index = 1;
foreach (var symbolAverageCorrelation in symbolsAverageCorrelation)
{
	Console.WriteLine($"{index}:\t{symbolAverageCorrelation.Key}:\t{symbolAverageCorrelation.Value}");
	index++;
}

Environment.Exit(0);

// Prints out the most correlated assets
foreach (var best in bestCorrelations)
{
	//whitelist symbols
	if (true)
	{
		//whitelistedSymbols.Contains(best.Key)) {

		Console.WriteLine($"{best.Key} Correlations:");

		foreach (var cor in best.Value)
		{
			Console.WriteLine($"{cor.Item1}: {cor.Item2:F2}");
		}

		Console.WriteLine();
	}
}

Console.ReadLine();


static double CalculateCorrelation(List<(DateTime, double)> data1, List<(DateTime, double)> data2)
{
	return Correlation.Pearson(data1.Select(x => x.Item2), data2.Select(x => x.Item2));


	double sum1 = 0, sum2 = 0, sum1sq = 0, sum2sq = 0, psum = 0;
	int n = Math.Min(data1.Count, data2.Count);

	for (int i = 0; i < n; i++)
	{
		double x = data1[i].Item2;
		double y = data2[i].Item2;

		sum1 += x;
		sum2 += y;
		sum1sq += x * x;
		sum2sq += y * y;
		psum += x * y;
	}

	double num = psum - (sum1 * sum2 / n);
	double den = Math.Sqrt((sum1sq - sum1 * sum1 / n) * (sum2sq - sum2 * sum2 / n));
	if (den == 0) return 0; // handle divide by zero case
	return num / den;
}
