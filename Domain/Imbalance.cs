namespace TradeProcessor.Domain;

public record Imbalance(
	decimal High,
	decimal Low,
	BiasType BiasType,
	GapType GapType,
	DateTime? OpenDateTime = default,
	DateTime? CloseDateTime = default) : IImbalance;
