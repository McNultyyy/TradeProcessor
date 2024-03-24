using FluentResults;

namespace TradeProcessor.Domain
{
	public enum SymbolType
	{
		Spot, Perpetual
	}

	public record Symbol(string Base, string Quote, string? Exchange = null, SymbolType? SymbolType = null) : IComparable<Symbol>
	{
		public string Base { get; } = Base;
		public string Quote { get; } = Quote;
		public string? Exchange { get; } = Exchange;
		public SymbolType? SymbolType { get; } = SymbolType;

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

			if (symbol.EndsWith("BTC"))
				return Result.Ok(
					new Symbol(
						symbol.Split("BTC").First(),
						"BTC"));

			return Result.Fail($"Unknown Quote instrument for Symbol: {symbol}");
		}

		public static Result<Symbol> Create(string symbol, string exchange, SymbolType symbolType)
		{
			if (symbol.EndsWith("USDT"))
				return Result.Ok(
					new Symbol(
						symbol.Split("USDT").First(),
						"USDT",
						exchange,
						symbolType));

			if (symbol.EndsWith("BTC"))
				return Result.Ok(
					new Symbol(
						symbol.Split("BTC").First(),
						"BTC",
						exchange,
						symbolType));

			return Result.Fail($"Unknown Quote instrument for Symbol: {symbol}");
		}

		public int CompareTo(Symbol? other)
		{
			if (ReferenceEquals(this, other))
			{
				return 0;
			}

			if (ReferenceEquals(null, other))
			{
				return 1;
			}

			var baseComparison = string.Compare(Base, other.Base, StringComparison.Ordinal);
			if (baseComparison != 0)
			{
				return baseComparison;
			}

			return string.Compare(Quote, other.Quote, StringComparison.Ordinal);
		}
	}
}
