// See https://aka.ms/new-console-template for more information

using System.Dynamic;
using System.Globalization;
using System.Xml.Linq;
using Binance.Net;
using Binance.Net.Interfaces.Clients;
using CsvHelper;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");


var serviceCollection = new ServiceCollection();
serviceCollection.AddBinance();

var serviceProvider = serviceCollection.BuildServiceProvider();

var binanceClient = serviceProvider.GetRequiredService<IBinanceRestClient>();

var symbols = (await binanceClient.SpotApi.CommonSpotClient.GetSymbolsAsync()).Data.Select(x => x.Name)
	.Where(x => x.EndsWith("USDT"))
	.Take(10);

var dict = new Dictionary<string, List<(DateTime openTime, decimal closingPrice)>>();

foreach (var symbol in symbols)
{
	var list = (await binanceClient.SpotApi.CommonSpotClient.GetKlinesAsync(symbol, TimeSpan.FromDays(1),
			DateTime.Now - TimeSpan.FromDays(30), DateTime.Now))
		.Data
		.Select(x => (x.OpenTime, x.ClosePrice.Value))
		.ToList();

	if (list.Any())
		dict.Add(symbol, list);
}


var records = new List<dynamic>();

var numberOfDates = dict.Max(x => x.Value.Count);

for (int i = 0; i < numberOfDates - 1; i++)
{
	var item = new ExpandoObject() as IDictionary<string, object>;

	item["Date"] = dict.First().Value[i].openTime;

	foreach (var (symbol, list) in dict)
	{
		item[symbol] = list[i].closingPrice;
	}

	records.Add(item);
}

var g = "";


await using var writer = new StreamWriter("./crypto.csv");
await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

await csv.WriteRecordsAsync(records);
