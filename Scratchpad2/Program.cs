// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradeProcessor.Core;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Exchange;
using TradeProcessor.Domain.Risk;
using TradeProcessor.Domain.Services;
using TradeProcessor.Domain.Stoploss;

var host = Host
	.CreateDefaultBuilder()
	.ConfigureServices((context, services) =>
	{
		services.AddTradeProcessorCore(context.Configuration);
		services.AddLogging(l => l.AddConsole());
	})
	.Build();

await host.StartAsync();

var restService = host.Services.GetRequiredService<IExchangeRestClient>();
var pdFinder = host.Services.GetRequiredService<PDArrayFinder>();
var tradeParser = host.Services.GetRequiredService<TradeParser>();

var symbols = (await restService.GetSymbols()).Value;
//.Take(50);
var interval = TimeSpan.FromDays(1);

await restService.CancelAllOrders();

try
{
	foreach (var symbol in symbols)
	{
		var pivots = (await pdFinder.Find(symbol, interval)).Take(50);

		var closestPivots = new[]
		{
			pivots.FirstOrDefault(x => x.biasType is BiasType.Bearish),
			//pivots.FirstOrDefault(x => x.biasType is BiasType.Bullish)
		}.Where(x => x != default);

		foreach (var pivot in closestPivots)
		{
			var tradeTicket = await tradeParser.Parse(symbol, pivot.biasType, takeProfit: null, stoploss: "5%",
				limitPrice: pivot.price, riskPerTrade: "20", interval: TimeSpan.FromDays(1), setStoploss: true);

			var result = await restService.PlaceOrder(tradeTicket);
		}
	}
}
catch (Exception ex)
{
	var g = "";
}


//var result = await restService.PlaceOrder(new Symbol("SOL", "USDT"), BiasType.Bullish, 10, 55, true, 65, 50);

await host.StopAsync();
