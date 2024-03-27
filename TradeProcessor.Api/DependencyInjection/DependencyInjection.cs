using AspNetCore.Authentication.ApiKey;
using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Extensions;
using Hangfire.InMemory;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Filters;
using TradeProcessor.Api.Authentication;
using TradeProcessor.Api.Examples;
using TradeProcessor.Api.Jobs;

namespace TradeProcessor.Api.DependencyInjection
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddTradeProcessorHangfire(this IServiceCollection services,
			string? connectionString)
		{
			services.AddHangfire(x =>
			{
				if (connectionString is null)
				{
					x.UseInMemoryStorage(new InMemoryStorageOptions()
					{
						MaxExpirationTime =
							TimeSpan.FromDays(7) // todo: consider whether we should make this null/infinite
					});
				}
				else
				{
					x.UseSqlServerStorage(connectionString);
				}

				x.UseConsole();
			});

			services.AddHangfireServer(options =>
			{
				// set to Int.MaxValue, but not when running locally otherwise it thread starves the process
				options.WorkerCount = connectionString is not null // having the connection string assumes we're not local 
					? Int32.MaxValue
					: 5;
			});
			services.AddHangfireConsoleExtensions();

			services.AddTransient<FailedJobsCleanupJob>();

			return services;
		}

		public static IServiceCollection AddTradeProcessorAuthorization(this IServiceCollection services)
		{
			services.AddAuthorization(options =>
			{
				var authenticationSchemes = new[]
				{
					AuthenticationSchemes.ApiKeyInHeader, AuthenticationSchemes.ApiKeyInQuery,
					AuthenticationSchemes.ApiKeyInRequest
				};

				var multiSchemePolicy = new AuthorizationPolicyBuilder(authenticationSchemes)
					.RequireAuthenticatedUser()
					.Build();

				options.FallbackPolicy = multiSchemePolicy;
			});

			return services;
		}

		public static IServiceCollection AddTradeProcessorAuthentication(this IServiceCollection services)
		{
			services.AddAuthentication()
				.AddApiKeyInHeader<StaticApiKeyProvider>(AuthenticationSchemes.ApiKeyInHeader, x =>
				{
					x.KeyName = "X-API-KEY";
					x.Realm = "Trade Processor";
				})
				.AddApiKeyInQueryParams<StaticApiKeyProvider>(AuthenticationSchemes.ApiKeyInQuery, x =>
				{
					x.KeyName = "apiKey";
					x.Realm = "Trade Processor";
				})
				.AddApiKeyInRequestBody<StaticApiKeyProvider>()
				;

			return services;
		}

		public static IServiceCollection AddTradeProcessorSwagger(this IServiceCollection services)
		{
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen(options =>
			{
				options.ExampleFilters();
			});
			services.AddSwaggerExamplesFromAssemblies(typeof(ExampleFvgChaserRequest).Assembly);

			return services;
		}
	}
}
