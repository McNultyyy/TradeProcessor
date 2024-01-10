using Microsoft.Extensions.DependencyInjection;
using TradeProcessor.Domain.Services;
using TradeProcessor.Domain.Stoploss;
using TradeProcessor.Domain.TechnicalAnalysis;

namespace TradeProcessor.Domain.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTradeProcessorDomain(this IServiceCollection services)
		{
			services.AddTransient<FvgChaser>();
			services.AddTransient<AverageTrueRangeProvider>();
			services.AddTransient<StoplossStrategyFactory>();

			return services;
		}
	}
}
