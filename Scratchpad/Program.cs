using System.Globalization;
using Binance.Net.Clients;
using Binance.Net.Interfaces;
using Scratchpad;
using Scratchpad.Entities;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Candles;
using TradeProcessor.Domain.Services;


Console.WriteLine("Do Work");


var interval = TimeSpan.FromHours(24);

var endDate = DateTime.Today - interval;
var orderBlockStartDate = endDate - (10 * interval);
var imbalanceStartDate = endDate - (3 * interval);

var binanceClient = new BinanceRestClient() { };

var symbols = (binanceClient.SpotApi.ExchangeData.GetTickersAsync())
	.Result
	.Data
	.Select(x => x.Symbol)
	.Where(x =>
		x.EndsWith("USDT")
		&& !x.EndsWith("UPUSDT")
		&& !x.EndsWith("DOWNUSDT"))
	.OrderBy(x => x)
	.Select(x => x)
	.ToList();

await LoadData();

await using var db = new PricesContext();
//var finder = new PivotTakenFinder(new SqliteDataProvider(db));

//await finder.FindPivotsTaken("FTMUSDT", TimeRange.Monthly);

await FindAndSaveOrderBlocks();


await FindAndSaveImbalances();




Console.WriteLine("done");
Console.ReadLine();

async Task LoadData()
{
	await using var db = new PricesContext();

	var interval = TimeSpan.FromHours(24);

	var endDate = DateTime.Today - interval;
	var startDate =
		//DateTime.ParseExact("2023-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
		endDate - (10 * interval);

	var binanceClient = new BinanceRestClient() { };

	var symbols = (binanceClient.SpotApi.ExchangeData.GetTickersAsync())
		.Result
		.Data
		.Select(x => x.Symbol)
		.Where(x =>
			x.EndsWith("USDT")
			&& !x.EndsWith("UPUSDT")
			&& !x.EndsWith("DOWNUSDT"))
		.OrderBy(x => x)
		.Select(x => x)
		.ToList();

	Console.WriteLine($"Found {symbols.Count} symbols");

	var isDataLoaded = false;
	if (!isDataLoaded)
	{
		foreach (var symbol in symbols)
		{
			var binanceKlines =
				(await binanceClient.SpotApi.ExchangeData.GetKlinesAsync(
					symbol,
					BinanceKlineIntervalFromTimeSpan(interval),
			startDate, endDate))
			.Data;

			var candles = binanceKlines
				.Select(FromBinanceKline)
				.OrderBy(x => x.OpenDateTime);

			foreach (var candle in candles)
			{
				try
				{
					var candleEntity = ToCandleEntity(candle, symbol);

					if ((await db.Candles.FindAsync(candleEntity.Id)) is null)
						await db.Candles.AddAsync(candleEntity);
				}
				catch (Exception ex)
				{
					var g = "";
				}
			}


			await db.SaveChangesAsync();

			Console.WriteLine($"Saved data for {symbol}");
		}
	}
}

async Task FindMonthlyPivotsTaken()
{
	var db = new PricesContext();

	var orderBlockFinder = new OrderBlockFinder(new SqliteDataProvider(db));

	var bullishOrderBlocks = new List<OrderBlock>();
	var bearishOrderBlocks = new List<OrderBlock>();

	foreach (var symbol in symbols.Select(x => Symbol.Create(x).Value))
	{
		Console.WriteLine($"Looking for OrderBlocks for {symbol}...");
		var bullishOrderBlock = await orderBlockFinder.FindOrderBlock(symbol, orderBlockStartDate, endDate, interval, BiasType.Bullish);
		var bearishOrderBlock = await orderBlockFinder.FindOrderBlock(symbol, orderBlockStartDate, endDate, interval, BiasType.Bearish);

		if (bullishOrderBlock is not null)
		{
			Console.WriteLine("Found Bullish OrderBlock!");
			bullishOrderBlocks.Add(bullishOrderBlock);
			await db.OrderBlocks.AddAsync(ToOrderBlockEntity(bullishOrderBlock, symbol));
		}

		if (bearishOrderBlock is not null)
		{
			Console.WriteLine("Found Bearish OrderBlock!");
			bearishOrderBlocks.Add(bearishOrderBlock);
			await db.OrderBlocks.AddAsync(ToOrderBlockEntity(bearishOrderBlock, symbol));
		}

		Console.WriteLine();
	}

	await db.SaveChangesAsync();
}

async Task FindAndSaveOrderBlocks()
{
	var db = new PricesContext();

	var orderBlockFinder = new OrderBlockFinder(new SqliteDataProvider(db));

	var bullishOrderBlocks = new List<OrderBlock>();
	var bearishOrderBlocks = new List<OrderBlock>();

	foreach (var symbol in symbols.Select(x => Symbol.Create(x).Value))
	{
		Console.WriteLine($"Looking for OrderBlocks for {symbol}...");
		var bullishOrderBlock = await orderBlockFinder.FindOrderBlock(symbol, orderBlockStartDate, endDate, interval, BiasType.Bullish);
		var bearishOrderBlock = await orderBlockFinder.FindOrderBlock(symbol, orderBlockStartDate, endDate, interval, BiasType.Bearish);

		if (bullishOrderBlock is not null)
		{
			Console.WriteLine("Found Bullish OrderBlock!");
			bullishOrderBlocks.Add(bullishOrderBlock);
			await db.OrderBlocks.AddAsync(ToOrderBlockEntity(bullishOrderBlock, symbol));
		}

		if (bearishOrderBlock is not null)
		{
			Console.WriteLine("Found Bearish OrderBlock!");
			bearishOrderBlocks.Add(bearishOrderBlock);
			await db.OrderBlocks.AddAsync(ToOrderBlockEntity(bearishOrderBlock, symbol));
		}

		Console.WriteLine();
	}

	await db.SaveChangesAsync();
}

async Task FindAndSaveImbalances()
{
	var db = new PricesContext();

	var imbalanceFinder = new ImbalanceFinder(new SqliteDataProvider(db));


	foreach (var symbol in symbols.Select(x => Symbol.Create(x).Value))
	{
		Console.WriteLine($"Looking for Imbalances for {symbol}...");
		var imbalances = await imbalanceFinder.FindImbalances(symbol, imbalanceStartDate, endDate, GapType.Price);

		if (imbalances.Any())
		{
			Console.WriteLine("Found Imbalances!");
			await db.Imbalances.AddRangeAsync(imbalances.Select(imbalance => ToImbalanceEntity(imbalance, symbol)));
		}

		Console.WriteLine();
	}

	await db.SaveChangesAsync();
}

Candle FromBinanceKline(IBinanceKline kline) =>
	new(
		kline.OpenPrice,
		kline.HighPrice,
		kline.LowPrice,
		kline.ClosePrice,
		kline.OpenTime,
		kline.CloseTime);

Binance.Net.Enums.KlineInterval BinanceKlineIntervalFromTimeSpan(TimeSpan timeSpan)
{
	return (Binance.Net.Enums.KlineInterval)timeSpan.TotalSeconds;
}

CandleEntity ToCandleEntity(Candle candle, string symbol)
{
	return new CandleEntity()
	{
		Symbol = Symbol.Create(symbol).Value, //todo: refactor where we create the symbol

		Open = candle.Open,
		High = candle.High,
		Low = candle.Low,
		Close = candle.Close,
		OpenDateTime = candle.OpenDateTime,
		CloseDateTime = candle.CloseDateTime
	};
}

OrderBlockEntity ToOrderBlockEntity(OrderBlock orderBlock, Symbol symbol)
{
	return new OrderBlockEntity()
	{
		Id = new Guid(),
		Symbol = symbol,

		Open = orderBlock.Open,
		High = orderBlock.High,
		Low = orderBlock.Low,
		Close = orderBlock.Close,
		OpenDateTime = orderBlock.OpenDateTime,
		CloseDateTime = orderBlock.CloseDateTime,

		LiquidityWick =
			orderBlock.LiquidityTakingCandle.IsBullishCandle()
				? orderBlock.LiquidityCandle.High
				: orderBlock.LiquidityCandle.Low,
	};
}

ImbalanceEntity ToImbalanceEntity(Imbalance imbalance, Symbol symbol)
{
	return new ImbalanceEntity()
	{
		Symbol = symbol,

		High = imbalance.High,
		Low = imbalance.Low,

		BiasType = imbalance.BiasType,
		GapType = imbalance.GapType,
	};
}
