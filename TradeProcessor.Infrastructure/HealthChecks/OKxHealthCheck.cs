using Microsoft.Extensions.Diagnostics.HealthChecks;
using OKX.Api;

namespace TradeProcessor.Infrastructure.HealthChecks
{
	public class OKxHealthCheck : IHealthCheck
	{
		private readonly OKXRestApiClient _restClient;

		public OKxHealthCheck(OKXRestApiClient restClient)
		{
			_restClient = restClient;
		}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
			CancellationToken cancellationToken = new CancellationToken())
		{
			// todo: implement a ping healthcheck to OKx
			var isHealthy = (await _restClient.PublicData.GetServerTimeAsync(cancellationToken)).Success;
			if (isHealthy)
			{
				return HealthCheckResult.Healthy("A healthy result.");
			}

			return new HealthCheckResult(context.Registration.FailureStatus, "An unhealthy result.");
		}
	}
}
