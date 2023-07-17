using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using TradeProcessor.Api.Contracts;

namespace TradeProcessor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {

        private readonly ILogger<HealthController> _logger;
        private HealthCheckService _healthCheckService;

        public HealthController(ILogger<HealthController> logger, HealthCheckService healthCheckService)
        {
            _logger = logger;
            _healthCheckService = healthCheckService;
        }

        [HttpGet]
        public async Task<IActionResult> Healthcheck()
        {
            try
            {
                var report = await _healthCheckService.CheckHealthAsync();
                var result = new
                {
                    status = report.Status.ToString(),
                    errors = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description?.ToString()
                    })
                };

                return report.Status == HealthStatus.Healthy
                    ? Ok(result)
                    : StatusCode((int)HttpStatusCode.ServiceUnavailable, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }

        }
    }
}
