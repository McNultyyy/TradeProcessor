// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradeProcessor.Core;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.Exchange;
using TradeProcessor.Domain.Risk;
using TradeProcessor.Domain.Services;
using TradeProcessor.Domain.Stoploss;
using TradeProcessor.Infrastructure.Services.Binance;

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
var binanceClient = host.Services.GetRequiredService<BinanceExchangeRestClient>();

var symbols = (await restService.GetSymbols()).Value;
//.Take(50);
var interval = TimeSpan.FromDays(1);

await restService.CancelAllOrders();


/* place orders at pivots
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
*/

var now = DateTime.UtcNow;
var startDate = now - TimeSpan.FromDays(300);
var timeframe = TimeSpan.FromDays(1);

var binanceSymbols = (await binanceClient.GetSymbols()).Value;
foreach (var binanceSymbol in binanceSymbols)
{
	var symbol = Symbol.Create("KSMUSDT").Value;

	var symbolCandles = (await binanceClient.GetCandles(symbol, timeframe, startDate, now)).Value
		.OrderBy(x => x.OpenDateTime)
		.ToList();

	var vibs = new List<Imbalance>();

	for (int i = 2; i < symbolCandles.Count - 2; i++)
	{
		var (previousPrevious, previous, current) = (symbolCandles[i - 2], symbolCandles[i - 1], symbolCandles[i]);
		var threeCandles = new ThreeCandles(previousPrevious, previous, current);

		threeCandles.TryFindImbalances(out var imbalances);
		if (imbalances is not null)
		{
			var vib = imbalances.FirstOrDefault(x => x.GapType == GapType.Volume);
			if (vib is not null)
				vibs.Add(vib);
		}
	}


	var openVibs = vibs.Where(vib => symbolCandles.Any(candle =>
			candle.IsAfter(vib) &&
			candle.TradesThrough(vib)))
		.ToList();
}


//var result = await restService.PlaceOrder(new Symbol("SOL", "USDT"), BiasType.Bullish, 10, 55, true, 65, 50);

await host.StopAsync();
