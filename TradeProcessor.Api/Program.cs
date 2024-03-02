using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TradeProcessor.Api.Authorization;
using TradeProcessor.Api.DependencyInjection;
using TradeProcessor.Api.Healthcheck;
using TradeProcessor.Core;
using TradeProcessor.Infrastructure.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddControllers()
	.AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
	.AddTradeProcessorSwagger()
	.AddTradeProcessorHangfire()
	.AddTradeProcessorAuthentication()
	.AddTradeProcessorAuthorization()
	.AddTradeProcessorCore(builder.Configuration);

builder.Services.AddHealthChecks()
	.AddCheck("System", () => HealthCheckResult.Healthy())
	.AddCheck<BybitHealthCheck>("Bybit")
	.AddCheck<OKxHealthCheck>("OKx");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

/*
 * Configuring Hangfire Dashboard and Healthchecks are intentionally before the AuthN and AuthZ configuration.
 */
app.UseHangfireDashboard("/hangfire", new DashboardOptions()
{
	Authorization = new List<IDashboardAuthorizationFilter>()
	{
		new AllowAllAuthorizationFilter()
	}
});

app.UseHealthChecks("/health", new HealthCheckOptions()
{
	ResponseWriter = HealthCheckResponseWriter.Write
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
