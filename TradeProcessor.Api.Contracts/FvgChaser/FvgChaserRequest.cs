using TradeProcessor.Domain;

namespace TradeProcessor.Api.Contracts.FvgChaser;


public record FvgChaserRequest : IApiKeyProperty
{
	private static readonly IEnumerable<GapType> DefaultGapType = new[] { GapType.Price };

	public FvgChaserRequest(string Symbol,
		string Interval,
		decimal RiskPerTrade,
		string Stoploss,
		string? TakeProfit, // only the formatted string
		BiasType Bias,
		IEnumerable<GapType>? Gaps = null)
	{
		this.Symbol = Symbol;
		this.Interval = Interval;
		this.RiskPerTrade = RiskPerTrade;
		this.Stoploss = Stoploss;
		this.TakeProfit = TakeProfit;
		this.Bias = Bias;
		this.Gaps = Gaps ?? DefaultGapType;
	}


	public string ApiKey { get; set; }
	public string Symbol { get; init; }
	public string Interval { get; init; }
	public decimal RiskPerTrade { get; init; }
	public string Stoploss { get; init; }
	public string? TakeProfit { get; init; }
	public BiasType Bias { get; init; }
	public IEnumerable<GapType>? Gaps { get; init; }

	public void Deconstruct(out string Symbol, out string Interval, out decimal RiskPerTrade, out string Stoploss, out string? TakeProfit, // only the formatted string
		out BiasType Bias, out IEnumerable<GapType>? Gaps)
	{
		Symbol = this.Symbol;
		Interval = this.Interval;
		RiskPerTrade = this.RiskPerTrade;
		Stoploss = this.Stoploss;
		TakeProfit = this.TakeProfit;
		Bias = this.Bias;
		Gaps = this.Gaps;
	}
}
