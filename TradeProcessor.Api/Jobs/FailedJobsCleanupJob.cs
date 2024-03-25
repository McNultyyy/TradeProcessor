using Hangfire;
using Hangfire.States;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace TradeProcessor.Api.Jobs
{
	public class FailedJobsCleanupJob
	{
		private ILogger<FailedJobsCleanupJob> _logger;
		private IMonitoringApi _monitoringApi;
		private IBackgroundJobClient _client;
		private bool _shutdown;

		public FailedJobsCleanupJob(ILogger<FailedJobsCleanupJob> logger, IMonitoringApi monitoringApi, IBackgroundJobClient client)
		{
			_logger = logger;
			_monitoringApi = monitoringApi;
			_client = client;
		}

		public void Execute(IJobCancellationToken token)
		{
			using (_logger.BeginScope(new Dictionary<string, object> {{"labels.messageType", "FailedJobsCleanupJob"},}))
			{
				JobList<FailedJobDto> failedJobs;
				_logger.LogInformation("Executing Failed Hangfire Jobs Cleanup Job.");
				try
				{
					failedJobs = _monitoringApi.FailedJobs(0, 1000);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Could not get Failed jobs, Reason . {ex.Message} .");
					return;
				}

				if (failedJobs.Any())
				{
					using (token.ShutdownToken.Register(ShutdownReceived))
						Parallel.ForEach(failedJobs, (failedjob, state) =>
						{
							MarkJobAsDeleted(state, failedjob);
						});
					_logger.LogInformation("Succesfully deleted Failed jobs in Hangfire.");
				}
				else
				{
					_logger.LogInformation("Could not find Failed jobs in Hangfire to Delete.");
				}
			}
		}

		private void ShutdownReceived()
		{
			_shutdown = true;
		}

		private void MarkJobAsDeleted(ParallelLoopState state, KeyValuePair<string, FailedJobDto> failedJob)
		{
			if (_shutdown)
			{
				state.Break();
				return;
			}

			// we frequently run bots on the 5 minute time frame, so if orphaned bots are not killed within that timeframe
			// then we face the risk of "deleted" bots still opening orders
			if (DateTime.UtcNow - failedJob.Value.FailedAt > TimeSpan.FromMinutes(5))
			{
				var jobId = failedJob.Key;
				var deletedState = new DeletedState();

				try
				{
					var success = _client.ChangeState(jobId, deletedState, "Failed");
					if (!success)
					{
						_logger.LogWarning("Failed to change state of Job Id {0}", jobId);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Exception while deleting failed jobs in hangfire");
				}
			}
		}
	}
}
