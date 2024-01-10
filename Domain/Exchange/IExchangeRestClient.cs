﻿using FluentResults;
using TradeProcessor.Domain.Candles;

namespace TradeProcessor.Domain.Exchange
{
	public interface IExchangeRestClient
	{
		public Task<Result> PlaceOrder(Symbol symbol, BiasType bias, decimal quantity, decimal price, decimal? takeProfit);

		public Task<Result<IEnumerable<Candle>>> GetCandles(Symbol symbol, TimeSpan interval, DateTime from, DateTime to);

		public Task<Result<IEnumerable<Symbol>>> GetSymbols();
	}
}