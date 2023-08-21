using Bybit.Net;
using Bybit.Net.Objects.Options;
using CryptoExchange.Net.Authentication;

namespace TradeProcessor.Api.DependencyInjection
{
	public static class ServiceCollectionExtensions 
	{
		public static IServiceCollection AddBybit(this IServiceCollection services, IConfiguration configuration)
		{
			var (key, secret) = configuration
				.GetRequiredSection("Bybit")
				.GetTuple(
					x => x["Key"],
					x => x["Secret"]);

			var apiCredentials = new ApiCredentials(key, secret);

			services.AddBybit(
				restOptions => restOptions.DerivativesOptions.ApiCredentials = apiCredentials,
				socketOptions => socketOptions.DerivativesPublicOptions.ApiCredentials = apiCredentials,
				ServiceLifetime.Singleton);
			
			return services;
		}
	}
}
