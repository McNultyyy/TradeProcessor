using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using AspNetCore.Authentication.ApiKey;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TradeProcessor.Api.Authentication;
using TradeProcessor.Api.Authorization;
using TradeProcessor.Api.DependencyInjection;
using TradeProcessor.Api.FvgChaser;
using TradeProcessor.Api.HealthChecks;

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

builder.Services
	.AddControllers()
	.AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

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
	var multiSchemePolicy = new AuthorizationPolicyBuilder(
			"ApiKeyInHeader",
			"ApiKeyInQuery",
			"ApiKeyInRequest"
		)
		.RequireAuthenticatedUser()
		.Build();

	options.FallbackPolicy = multiSchemePolicy;
});

builder.Services.AddBybit(builder.Configuration);

builder.Services.AddHealthChecks()
	.AddCheck("System", () => HealthCheckResult.Healthy())
	.AddCheck<BybitHealthCheck>("Bybit");

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
	ResponseWriter = ((context, healthReport) =>
	{
		context.Response.ContentType = "application/json; charset=utf-8";

		var options = new JsonWriterOptions { Indented = true };

		using var memoryStream = new MemoryStream();
		using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
		{
			jsonWriter.WriteStartObject();
			jsonWriter.WriteString("status", healthReport.Status.ToString());
			jsonWriter.WriteStartObject("results");

			foreach (var healthReportEntry in healthReport.Entries)
			{
				jsonWriter.WriteStartObject(healthReportEntry.Key);
				jsonWriter.WriteString("status",
					healthReportEntry.Value.Status.ToString());
				jsonWriter.WriteString("description",
					healthReportEntry.Value.Description);
				jsonWriter.WriteStartObject("data");

				foreach (var item in healthReportEntry.Value.Data)
				{
					jsonWriter.WritePropertyName(item.Key);

					JsonSerializer.Serialize(jsonWriter, item.Value,
						item.Value?.GetType() ?? typeof(object));
				}

				jsonWriter.WriteEndObject();
				jsonWriter.WriteEndObject();
			}

			jsonWriter.WriteEndObject();
			jsonWriter.WriteEndObject();
		}

		return context.Response.WriteAsync(
			Encoding.UTF8.GetString(memoryStream.ToArray()));
	})
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
