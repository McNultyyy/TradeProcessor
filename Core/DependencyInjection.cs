using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.Console;
using TradeProcessor.Domain.DependencyInjection;
using TradeProcessor.Infrastructure.DependencyInjection;

namespace TradeProcessor.Core
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTradeProcessorCore(this IServiceCollection services,
			IConfiguration configuration)
		{
			services.AddLogging(logging =>
			{
				var appInsightsConnectionString = configuration.GetValue<string?>("APPLICATIONINSIGHTS_CONNECTION_STRING", null);
				if (appInsightsConnectionString is not null)
					logging.AddApplicationInsights(
						telemetryConfiguration => telemetryConfiguration.ConnectionString = appInsightsConnectionString,
						options => options.IncludeScopes = true);
			});

			services.AddTradeProcessorDomain();
			services.AddTradeProcessorInfrastructure(configuration);

			return services;
		}
	}
}
