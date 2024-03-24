using ListGenerator.ListGenerators;

namespace ListGenerator.FileGenerators
{
	public class TradingViewWatchlistFileGenerator : IFileGenerator
	{
		private readonly string _outputPath;

		public TradingViewWatchlistFileGenerator(string outputPath)
		{
			_outputPath = outputPath;
		}


		public async Task GenerateAsync(ISymbolsListGenerator symbolsListGenerator, string? fileName = null)
		{
			fileName ??= symbolsListGenerator.GetDefaultFileName() + ".txt";

			var symbols = await symbolsListGenerator.GenerateAsync();

			var fileLines = symbols.Select(Extensions.SymbolToTradingViewSymbol);

			await File.WriteAllLinesAsync(Path.Combine(_outputPath, fileName), fileLines);
		}
	}
}
