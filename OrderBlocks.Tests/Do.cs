using Binance.Net.Clients;
using Binance.Net.Interfaces;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using Bybit.Net.Objects.Models;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Candles;

namespace OrderBlocks.Tests
{
	public class Do
	{
		public async Task<OrderBlock> Work(string symbol, DateTime startDate, DateTime endDate, TimeSpan interval, BiasType bias)
		{
			var binanceClient = new BinanceRestClient() { };
			var bybitClient = new BybitRestClient();

			async Task<OrderBlock?> TryFindOrderBlock()
			{
				try
				{
					var binanceKlines =
						(await binanceClient.UsdFuturesApi.ExchangeData.GetKlinesAsync(
							symbol,
							BinanceKlineIntervalFromTimeSpan(interval),
							startDate, endDate))
						.Data;

					var bybitKlines =
						(await bybitClient.DerivativesApi.ExchangeData.GetKlinesAsync(
							Category.Linear,
							symbol,
							BybitKlineIntervalFromTimeSpan(interval),
							startDate, endDate))
						.Data;

					var candles = binanceKlines
						.Select(FromBinanceKline)
						.OrderBy(x => x.OpenDateTime);

					// abstract into `Liquidity`.
					// then include things like equal highs/lows etc.
					// also include filling FVGs?

					var pivots = candles.GetPivots()
						.Where(x =>
							x.PivotType == (bias is BiasType.Bearish ?
								PivotType.High :
								PivotType.Low))
						.ToList();

					var sequences = candles.GetSequencesStrict();

					var firstPivot = pivots.First();

					var firstCandleThatBreaksPivot = candles
						.Where(candle => candle.IsAfter(firstPivot.PivotCandle))
						.First(candle =>
							(bias is BiasType.Bearish
								? candle.HasHigherHighThan(firstPivot.PivotCandle) && candle.IsBullishCandle()
								: candle.HasLowerLowThan(firstPivot.PivotCandle) && candle.IsBearishCandle()));

					var sequenceWhichBrokeThePivot = sequences
						.First(sequence =>
							sequence.ContainsCandle(firstCandleThatBreaksPivot)
						);

					var firstCloseThatBreaksSequenceOpen = candles
						.Where(candle => candle.IsAfter(sequenceWhichBrokeThePivot))
						.First(candle =>
							bias is BiasType.Bearish
								? candle.ClosesBelowOpen(sequenceWhichBrokeThePivot)
								: candle.ClosesAboveOpen(sequenceWhichBrokeThePivot));


					var firstSequenceThatBreaksSequenceOpen = sequences
						.First(sequence =>
							sequence.ContainsCandle(firstCloseThatBreaksSequenceOpen)
						);

					var orderBlock = new OrderBlock(
						firstPivot.PivotCandle,
						sequenceWhichBrokeThePivot,
						firstSequenceThatBreaksSequenceOpen);

					return orderBlock;
				}
				catch (Exception ex)
				{
					return null;
				}
			}


			OrderBlock? orderBlock = null;
			while ((orderBlock = await TryFindOrderBlock()) is null &&
				   startDate < endDate)
			{
				startDate += interval;
			};

			return orderBlock;
		}

		Candle FromBinanceKline(IBinanceKline kline) =>
			new(
				kline.OpenPrice,
				kline.HighPrice,
				kline.LowPrice,
				kline.ClosePrice,
				kline.OpenTime,
				kline.CloseTime);

		Candle FromBybitKline(BybitKline kline) =>
			new(
				kline.OpenPrice,
				kline.HighPrice,
				kline.LowPrice,
				kline.ClosePrice,
				kline.OpenTime,
				kline.OpenTime + FromBybitKlineInterval(kline.Interval));

		TimeSpan FromBybitKlineInterval(Bybit.Net.Enums.KlineInterval klineInterval)
		{
			return TimeSpan.FromSeconds((int)klineInterval);
		}

		Bybit.Net.Enums.KlineInterval BybitKlineIntervalFromTimeSpan(TimeSpan timeSpan)
		{
			return (Bybit.Net.Enums.KlineInterval)timeSpan.TotalSeconds;
		}

		Binance.Net.Enums.KlineInterval BinanceKlineIntervalFromTimeSpan(TimeSpan timeSpan)
		{
			return (Binance.Net.Enums.KlineInterval)timeSpan.TotalSeconds;
		}
	}
}
