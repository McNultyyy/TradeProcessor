using AspNetCore.Authentication.ApiKey;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TradeProcessor.Api.Authentication;
using TradeProcessor.Api.Authorization;
using TradeProcessor.Api.FvgChaser;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(logging =>
{
	logging.ClearProviders();

	logging.AddConsole(opts =>
	{
		opts.IncludeScopes = true;
	});
	logging.AddApplicationInsights();
});
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfire(x =>
{
	x.UseInMemoryStorage();
	x.UseConsole();
});

builder.Services.AddHangfireServer(options =>
{

});



builder.Services.AddTransient<FvgChaser>();

builder.Services.AddAuthentication()
	.AddApiKeyInHeader<StaticApiKeyProvider>("ApiKeyInHeader", x =>
	{
		x.KeyName = "X-API-KEY";
		x.Realm = "Trade Processor";
	})
	.AddApiKeyInQueryParams<StaticApiKeyProvider>("ApiKeyInQuery", x =>
	{
		x.KeyName = "apiKey";
		x.Realm = "Trade Processor";
	})
	.AddApiKeyInRequestBody<StaticApiKeyProvider>()
	;


builder.Services.AddAuthorization(options =>
{
	var multiSchemePolicy = new AuthorizationPolicyBuilder(
			"ApiKeyInHeader",
			"ApiKeyInQuery",
			"ApiKeyInRequest"
		)
		.RequireAuthenticatedUser()
		.RequireAuthenticatedUser()
		.Build();

	options.FallbackPolicy = multiSchemePolicy;
});

builder.Services.AddHealthChecks()
	.AddCheck("System", () => HealthCheckResult.Healthy());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// In Local development we don't want to both with a Authorization
if (app.Environment.IsDevelopment())
{
	app.UseHangfireDashboard("/hangfire", new DashboardOptions()
	{
		Authorization = new List<IDashboardAuthorizationFilter>()
		{
			new AllowAllAuthorizationFilter()
		}
	});
}

// before auth
app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();

// We want to enable the dashboard after the UseAuthorization call in Production, we will need an API Key
if (!app.Environment.IsDevelopment())
{
	app.UseHangfireDashboard("/hangfire");
}

app.MapControllers();

app.Run();
