using Bybit.Net;
using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OKX.Api;
using OKX.Api.Authentication;
using TradeProcessor.Domain;
using TradeProcessor.Domain.Exchange;
using TradeProcessor.Infrastructure.Services.Bybit;
using TradeProcessor.Infrastructure.Services.OKx;

namespace TradeProcessor.Infrastructure.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTradeProcessorInfrastructure(this IServiceCollection services,
			IConfiguration configuration)
		{
			switch (configuration["Exchange"])
			{
				case "Bybit":
					services.AddBybit(configuration);
					services.AddSingleton<IExchangeRestClient, BybitExchangeRestClient>();
					services.AddSingleton<IExchangeSocketClient, BybitExchangeSocketClient>();
					break;

				case "OKx":
					services.AddOkx(configuration);
					services.AddSingleton<IExchangeRestClient, OKxExchangeRestClient>();
					services.AddSingleton<IExchangeSocketClient, OkxExchangeSocketClient>();
					break;

				default:
					throw new ArgumentOutOfRangeException();
			};

			return services;
		}

		private static IServiceCollection AddBybit(this IServiceCollection services, IConfiguration configuration)
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

		private static IServiceCollection AddOkx(this IServiceCollection services, IConfiguration configuration)
		{
			var (key, secret, passphrase) = configuration
				.GetRequiredSection("OKx")
				.GetTuple(
					x => x["Key"],
					x => x["Secret"],
					x => x["Passphrase"]);

			var okxApiCredentials = new OkxApiCredentials(key, secret, passphrase);

			services.AddTransient(sp =>
			{
				var okxRestClient = new OKXRestApiClient(
					sp.GetRequiredService<ILoggerFactory>().CreateLogger<OKXRestApiClient>(),
					new OKXRestApiClientOptions()
					{
						ApiCredentials = okxApiCredentials,
					});

				return okxRestClient;
			});

			services.AddSingleton(sp =>
			{
				var client = new OKXWebSocketApiClient(
					sp.GetRequiredService<ILoggerFactory>().CreateLogger<OKXWebSocketApiClient>(),
					new OKXWebSocketApiClientOptions()
					{
						ApiCredentials = okxApiCredentials
					});

				return client;
			});

			return services;
		}
	}
}
