namespace TradeProcessor.Api.Domain;

public record Imbalance(decimal High, decimal Low, BiasType BiasType, GapType GapType);
