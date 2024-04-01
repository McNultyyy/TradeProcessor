using Microsoft.Extensions.DependencyInjection;
using TradeProcessor.Domain.Risk;
using TradeProcessor.Domain.Services;
using TradeProcessor.Domain.Stoploss;
using TradeProcessor.Domain.TakeProfit;
using TradeProcessor.Domain.TechnicalAnalysis;

namespace TradeProcessor.Domain.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTradeProcessorDomain(this IServiceCollection services)
		{
			services.AddTransient<FvgChaser>();
			services.AddTransient<PDArrayFinder>();

			services.AddTransient<AverageTrueRangeProvider>();
			services.AddTransient<StoplossStrategyFactory>();
			services.AddTransient<RiskStrategyFactory>();
			services.AddTransient<TakeProfitStrategyFactory>();

			return services;
		}
	}
}
