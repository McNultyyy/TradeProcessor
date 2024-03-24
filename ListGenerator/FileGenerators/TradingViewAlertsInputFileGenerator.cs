using ListGenerator.ListGenerators;

namespace ListGenerator.FileGenerators
{
	public class TradingViewAlertsInputFileGenerator : IFileGenerator
	{
		private readonly string _outputPath;

		public TradingViewAlertsInputFileGenerator(string outputPath)
		{

			_outputPath = outputPath;
		}

		public async Task GenerateAsync(ISymbolsListGenerator symbolsListGenerator, string? fileName = null)
		{
			fileName ??= symbolsListGenerator.GetDefaultFileName() + ".csv";

			var symbols = await symbolsListGenerator.GenerateAsync();

			var fileLines = new[] { "symbol" }
				.Concat(
					symbols.Select(Extensions.SymbolToTradingViewSymbol));

			await File.WriteAllLinesAsync(Path.Combine(_outputPath, fileName), fileLines);
		}
	}
}
