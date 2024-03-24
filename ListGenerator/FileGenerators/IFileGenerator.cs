using ListGenerator.ListGenerators;

namespace ListGenerator.FileGenerators
{
	public interface IFileGenerator
	{
		Task GenerateAsync(ISymbolsListGenerator symbolsListGenerator, string? fileName = null);
	}
}
