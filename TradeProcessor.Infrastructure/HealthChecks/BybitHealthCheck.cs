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

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
		{
			var isHealthy = await _bybitRestClient.DerivativesApi.ExchangeData.GetServerTimeAsync(cancellationToken);

			if (isHealthy)
			{
				return HealthCheckResult.Healthy("A healthy result.");
			}

			return new HealthCheckResult(context.Registration.FailureStatus, "An unhealthy result.");
		}
	}
}
