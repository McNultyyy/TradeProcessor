﻿// See https://aka.ms/new-console-template for more information

using System.Dynamic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OKX.Api;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Exchange;
using TradeProcessor.Infrastructure.DependencyInjection;

var host = Host
	.CreateDefaultBuilder()
	.ConfigureServices((context, services) =>
	{
		services.AddTradeProcessorInfrastructure(context.Configuration);
		services.AddLogging(l => l.AddConsole());

	})
	.Build();

await host.StartAsync();

/*
var client = host.Services.GetRequiredService<OKXRestApiClient>();

var allSymbols = (await client.OrderBookTrading.MarketData.GetTickersAsync(OKX.Api.Enums.OkxInstrumentType.Swap))
	.Data
	.OrderByDescending(x => x.Volume)
	.Select(x => x.Instrument)
	.Take(5);

var dict = new Dictionary<string, List<(DateTime date, decimal price)>>();

foreach (var symbol in allSymbols)
{
	var history = await client.OrderBookTrading.MarketData.GetCandlesticksHistoryAsync(symbol, OKX.Api.Enums.OkxPeriod.OneDay, DateTimeOffset.UtcNow.AddMonths(-12).ToUnixTimeMilliseconds());

	dict.Add(symbol, history.Data.Select(x => (x.Time, x.Close)).ToList());
}



var records = new List<dynamic>();

foreach (var item in dict)
{
	dynamic recordItem = new ExpandoObject();

	recordItem.Date = item.Key;
}

*/



var restService = host.Services.GetRequiredService<IExchangeRestClient>();
var result = await restService.PlaceOrder("SOLUSDT", BiasType.Bullish, 10, 55, null);


await host.StopAsync();
