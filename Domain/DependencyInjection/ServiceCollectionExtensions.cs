using Microsoft.Extensions.DependencyInjection;
using TradeProcessor.Domain.Services;

namespace TradeProcessor.Domain.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTradeProcessorDomain(this IServiceCollection services)
		{
			services.AddTransient<FvgChaser>();

			return services;
		}
	}
}
