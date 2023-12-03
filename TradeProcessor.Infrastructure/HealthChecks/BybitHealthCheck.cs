using Bybit.Net.Interfaces.Clients;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TradeProcessor.Infrastructure.HealthChecks
{
	public class BybitHealthCheck : IHealthCheck
	{
		private readonly IBybitRestClient _bybitRestClient;

		public BybitHealthCheck(IBybitRestClient bybitRestClient)
		{
			_bybitRestClient = bybitRestClient;
		}

		public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
		{
			var isHealthy = _bybitRestClient.DerivativesApi.GetTimeOffset() is not null;


			if (isHealthy)
			{
				return Task.FromResult(
					HealthCheckResult.Healthy("A healthy result."));
			}

			return Task.FromResult(
				new HealthCheckResult(
					context.Registration.FailureStatus, "An unhealthy result."));
		}
	}
}
