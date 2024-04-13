using TradeProcessor.Domain;
using TradeProcessor.Domain.Risk;
using TradeProcessor.Domain.Stoploss;
using TradeProcessor.Domain.TakeProfit;

namespace TradeProcessor.Domain.Services;

public class TradeParser
{
	private readonly StoplossStrategyFactory _stoplossStrategyFactory;
	private readonly RiskStrategyFactory _riskStrategyFactory;
	private readonly TakeProfitStrategyFactory _takeProfitStrategyFactory;

	public TradeParser(StoplossStrategyFactory stoplossStrategyFactory, RiskStrategyFactory riskStrategyFactory, TakeProfitStrategyFactory takeProfitStrategyFactory)
	{
		_stoplossStrategyFactory = stoplossStrategyFactory;
		_riskStrategyFactory = riskStrategyFactory;
		_takeProfitStrategyFactory = takeProfitStrategyFactory;
	}

	public async Task<TradeTicket> Parse(Symbol symbol, BiasType biasType, string? takeProfit, string? stoploss,
		decimal limitPrice, string riskPerTrade, TimeSpan interval, bool setStoploss,
		(decimal low, decimal high) fvg = default)
	{
		var stoplossStrategy =
			await _stoplossStrategyFactory.GetStoploss(symbol, biasType, stoploss, limitPrice, interval, fvg);
		var stoplossDecimal = stoplossStrategy.Result();

		var takeProfitStrategy =
			_takeProfitStrategyFactory.GetTakeProfit(biasType, takeProfit, limitPrice, stoplossStrategy, fvg);
		var takeProfitDecimal = takeProfitStrategy?.Result() ?? null;

		var riskStrategy = await _riskStrategyFactory.GetRisk(riskPerTrade);
		var risk = riskStrategy.Result();

		var quantity = Math.Round(
			risk / Math.Abs(limitPrice - stoplossDecimal),
			3);

		return new TradeTicket(
			symbol, biasType, quantity, limitPrice, new StoplossOptions(setStoploss, stoplossDecimal),
			takeProfitDecimal);
	}
}

public record TradeTicket(
	Symbol Symbol,
	BiasType BiasType,
	decimal Quantity,
	decimal Price,
	StoplossOptions StoplossOptions,
	decimal? TakeProfit
);

public record StoplossOptions(
	bool setStoploss,
	decimal? stoploss)
{
	public static StoplossOptions NoStoploss() => new(false, null);
};
