using FluentResults;

namespace TradeProcessor.Domain
{
	public record Symbol(string Base, string Quote)
	{
		public string Base { get; } = Base;
		public string Quote { get; } = Quote;

		public override string ToString()
		{
			return $"{Base}{Quote}";
		}

		public static Result<Symbol> Create(string symbol)
		{
			if (symbol.EndsWith("USDT"))
				return Result.Ok(
					new Symbol(
						symbol.Split("USDT").First(),
						"USDT"));

			return Result.Fail($"Unknown Quote instrument for Symbol: {symbol}");
		}
	}
}
