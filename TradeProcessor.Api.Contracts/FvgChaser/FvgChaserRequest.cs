using TradeProcessor.Domain;

namespace TradeProcessor.Api.Contracts.FvgChaser;


public record FvgChaserRequest : IApiKeyProperty
{
	private static readonly IEnumerable<GapType> DefaultGapType = new[] { GapType.Price };

	public FvgChaserRequest(string symbol,
		string interval,
		string riskPerTrade,
		string stoploss,
		string? takeProfit, // only the formatted string
		BiasType bias,
		int? numberOfTrades = 0,
		IEnumerable<GapType>? gaps = null)
	{
		Symbol = symbol;
		Interval = interval;
		RiskPerTrade = riskPerTrade;
		Stoploss = stoploss;
		TakeProfit = takeProfit;
		Bias = bias;
		NumberOfTrades = numberOfTrades;
		Gaps = gaps ?? DefaultGapType;
	}


	public string ApiKey { get; set; }
	public string Symbol { get; init; }
	public string Interval { get; init; }
	public string RiskPerTrade { get; init; }
	public string Stoploss { get; init; }
	public string? TakeProfit { get; init; }
	public BiasType Bias { get; init; }
	public int? NumberOfTrades { get; init; }
	public IEnumerable<GapType>? Gaps { get; init; }

	public void Deconstruct(out string symbol, out string interval, out string riskPerTrade, out string stoploss, out string? takeProfit, // only the formatted string
		out BiasType bias, out int? numberOfTrades, out IEnumerable<GapType>? gaps)
	{
		symbol = Symbol;
		interval = Interval;
		riskPerTrade = RiskPerTrade;
		stoploss = Stoploss;
		takeProfit = TakeProfit;
		bias = Bias;
		numberOfTrades = NumberOfTrades;
		gaps = Gaps;
	}
}
