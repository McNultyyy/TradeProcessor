using System.Text.Json.Serialization;
using AspNetCore.Authentication.ApiKey;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Swashbuckle.AspNetCore.Filters;
using TradeProcessor.Api.Authentication;
using TradeProcessor.Api.Authorization;
using TradeProcessor.Api.Examples;
using TradeProcessor.Api.FvgChaser;
using TradeProcessor.Api.Healthcheck;
using TradeProcessor.Core;
using TradeProcessor.Infrastructure.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddControllers()
	.AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblies(typeof(ExampleFvgChaserRequest).Assembly);

builder.Services.AddHangfire(x =>
{
	x.UseInMemoryStorage();
	x.UseConsole();
});

builder.Services.AddHangfireServer(options =>
{

});



builder.Services.AddAuthentication()
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

builder.Services.AddAuthorization(options =>
{
	var authenticationSchemes = new[]
	{
		AuthenticationSchemes.ApiKeyInHeader,
		AuthenticationSchemes.ApiKeyInQuery,
		AuthenticationSchemes.ApiKeyInRequest
	};

	var multiSchemePolicy = new AuthorizationPolicyBuilder(authenticationSchemes)
		.RequireAuthenticatedUser()
		.Build();

	options.FallbackPolicy = multiSchemePolicy;
});


builder.Services.AddTradeProcessorCore(builder.Configuration);

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
