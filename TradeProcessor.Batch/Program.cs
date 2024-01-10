using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradeProcessor.Api.Contracts;
using TradeProcessor.Api.Contracts.FvgChaser;
using TradeProcessor.Core;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.Exchange;
using TradeProcessor.Domain.Services;
using TradeProcessor.Infrastructure.DependencyInjection;

var host = Host
	.CreateDefaultBuilder()
	.ConfigureServices((context, services) =>
	{
		services.AddTradeProcessorCore(context.Configuration);
		services.AddLogging(l => l.AddConsole());

	})
	.Build();

await host.StartAsync();


var restClient = host.Services.GetRequiredService<IExchangeRestClient>();

var symbolsResult = await restClient.GetSymbols();
var symbols = symbolsResult.Value.Where(x => x.Quote is "USDT").Reverse();


var url = "https://tradeprocessor-api4983254c.azurewebsites.net/trade?apiKey=123";
var httpClient = new HttpClient();


var list = new List<string>();

foreach (var symbol in symbols)
{
	var mayDateTime = DateTime.ParseExact("01/05/2022", "dd/MM/yyyy", null);
	var candles = (await restClient.GetCandles(symbol, TimeSpan.FromDays(1), mayDateTime, DateTime.Now)).Value;

	var monthlyCandles = CandleFactory.CreateMonthlyCandles(candles).Value;

	var mayCandle = monthlyCandles.FirstOrDefault(x => x.OpenDateTime.Month == 5 && x.OpenDateTime.Year == 2022);

	if (mayCandle is not null
		&&
		candles
		.Where(x => x.OpenDateTime.Year == 2023)
		.None(x => (x.High > mayCandle.Close)))
	{

		list.Add(symbol.ToString());
		var g = "";
		//var request = new FvgChaserRequest(symbol.ToString(), "1D", 10m, "5%", null, BiasType.Bullish);
		//var response = await httpClient.PostAsJsonAsync(url, request);

	}
}

await File.WriteAllLinesAsync("list.txt", list);
