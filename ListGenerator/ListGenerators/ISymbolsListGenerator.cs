using TradeProcessor.Domain;

namespace ListGenerator.ListGenerators
{
	public interface ISymbolsListGenerator
	{
		Task<IEnumerable<Symbol>> GenerateAsync();
	}
}
