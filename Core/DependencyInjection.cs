using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeProcessor.Domain.DependencyInjection;
using TradeProcessor.Infrastructure.DependencyInjection;

namespace TradeProcessor.Core
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTradeProcessorCore(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddLogging();

			services.AddTradeProcessorDomain();
			services.AddTradeProcessorInfrastructure(configuration);

			return services;
		}
	}
}
