﻿using TradeProcessor.Api.Authentication;
using TradeProcessor.Domain;

namespace TradeProcessor.Api.Contracts;

public record FvgChaserRequest(
    string Symbol,
    string Interval,
    decimal RiskPerTrade,
    string Stoploss,
    string? TakeProfit, // only the formatted string
    BiasType Bias) : IApiKeyProperty
{
	public string ApiKey { get; set; }
}
