// Copyright 2016-2021, Pulumi Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Pulumi;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppService.Inputs;
using Pulumi.AzureNative.Sql;
using Pulumi.AzureNative.Sql.Inputs;
using Environment = System.Environment;

namespace Infrastructure;

class AppServiceStack : Stack
{
	public AppServiceStack()
	{
		var resourceGroup = new ResourceGroup("TradeProcessor-rg");

		var appServicePlan = new AppServicePlan("TradeProcessor-sp", new AppServicePlanArgs
		{
			Name = "TradeProcessor-sp",
			ResourceGroupName = resourceGroup.Name,
			Kind = "App",
			Sku = new SkuDescriptionArgs {Tier = "Standard", Name = "S1",},
		});


		var sqlServerAdminLogin = "4dminUs3r";
		var sqlServerAdminPassword = "MyPassword123";
		var sqlServer = new Server("TradeProcessor-sqlserver", new ServerArgs
		{
			ServerName = "TradeProcessor-sqlserver",
			AdministratorLogin = sqlServerAdminLogin,
			AdministratorLoginPassword = sqlServerAdminPassword,
			Location = "West Europe",
			PublicNetworkAccess = "Disabled",
			ResourceGroupName = resourceGroup.Name,
		});

		/*
			Family              Sku            Edition                     Capacity    Unit
		    Free                Free           Free                        5           DTU     True
			Basic               Basic          Basic                       5           DTU     True
			S0                  Standard       Standard                    10          DTU     True
		 */

		var sqlDatabase = new Database("TradeProcessor-database",
			new DatabaseArgs
			{
				DatabaseName = "TradeProcessor-database",
				Location = "West Europe",
				ResourceGroupName = resourceGroup.Name,
				ServerName = sqlServer.Name,
				ZoneRedundant = false,
				Sku = new SkuArgs {Capacity = 10, Family = "Standard", Name = "S0", Tier = "Standard"}
			});


		var appInsights = new Component("appInsights",
			new ComponentArgs
			{
				ResourceName = "TradeProcessor-ai",
				ResourceGroupName = resourceGroup.Name,
				ApplicationType = "web",
				Kind = "web",
				IngestionMode = IngestionMode.ApplicationInsights
			});
		
		string? okxKey;
		string? okxSecret;
		string? okxPassphrase;
		IList<string>? apiKeys;

		if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("OKX_KEY")))
		{
			Console.WriteLine("Reading secrets from Environment");

			okxKey = Environment.GetEnvironmentVariable("OKX_KEY");
			okxSecret = Environment.GetEnvironmentVariable("OKX_SECRET`");
			okxPassphrase = Environment.GetEnvironmentVariable("OKX_PASSPHRASE");
			apiKeys = Environment.GetEnvironmentVariable("OKX_KEY").Split(",");
		}
		else
		{
			Console.WriteLine("Reading secrets from local development file");

			var appSettingsJson = JsonSerializer.Deserialize<JsonNode>(
				File.ReadAllText(
					"C:\\Users\\willi\\Projects\\TradeProcessor\\TradeProcessor.Api\\appsettings.Development.json"),
				new JsonSerializerOptions {ReadCommentHandling = JsonCommentHandling.Skip});
			
			(okxKey, okxSecret, okxPassphrase, apiKeys) = (
				appSettingsJson["OKx"]["Key"].GetValue<string>(),
				appSettingsJson["OKx"]["Secret"].GetValue<string>(),
				appSettingsJson["OKx"]["Passphrase"].GetValue<string>(),
				appSettingsJson["ApiKeys"].AsArray().Select(x => x.GetValue<string>()).ToList());
		}

		var appServiceArgs = new AppServiceArgs
		{
			Name = "TradeProcessor-api",
			AppServicePlanId = appServicePlan.Id,
			ResourceGroupName = resourceGroup.Name,
			AppSettings = new InputMap<string>
			{
				{"APPINSIGHTS_INSTRUMENTATIONKEY", appInsights.InstrumentationKey},
				{
					"APPLICATIONINSIGHTS_CONNECTION_STRING",
					appInsights.InstrumentationKey.Apply(key => $"InstrumentationKey={key}")
				},
				{"ApplicationInsightsAgent_EXTENSION_VERSION", "~2"},
				{"Exchange", "OKx"},
				{"Bybit:Key", "test"},
				{"Bybit:Secret", "test"},
				{"OKx:Key", okxKey},
				{"OKx:Secret", okxSecret},
				{"OKx:Passphrase", okxPassphrase},
			},
			ConnectionStrings = new InputList<AppServiceConnectionStringArgs>
			{
				new AppServiceConnectionStringArgs
				{
					Name = "HangfireDatabase",
					Value = Output
						.Tuple(sqlServer.Name, sqlDatabase.Name)
						.Apply(t =>
							$"Server=tcp:{t.Item1}.database.windows.net,1433;" +
							$"Initial Catalog={t.Item2};" +
							"Persist Security Info=False;" +
							$"User ID={sqlServerAdminLogin};" +
							$"Password={sqlServerAdminPassword};" +
							"MultipleActiveResultSets=False;" +
							"Encrypt=True;" +
							"TrustServerCertificate=False;" +
							"Connection Timeout=30;"),
					Type = "SQLAzure" // or SQLServer
				}
			},
			SiteConfig =
				new AppServiceSiteConfigArgs {AlwaysOn = true, HealthCheckPath = "/health", MinTlsVersion = "1.2",},
			Location = "West Europe",
			Logs = new AppServiceLogsArgs
			{
				DetailedErrorMessagesEnabled = true,
				FailedRequestTracingEnabled = true,
				HttpLogs = new AppServiceLogsHttpLogsArgs
				{
					FileSystem = new AppServiceLogsHttpLogsFileSystemArgs
					{
						RetentionInDays = 1, RetentionInMb = 35
					},
				},
				ApplicationLogs = new AppServiceLogsApplicationLogsArgs {FileSystemLevel = "Warning"}
			}
		};

		for (int i = 0; i < apiKeys.Count; i++)
		{
			appServiceArgs.AppSettings.Add(
				$"ApiKeys__{i}", apiKeys[i]);
		}

		var app = new AppService(
			"TradeProcessor-api",
			appServiceArgs,
			new CustomResourceOptions());

		this.Endpoint = app.DefaultSiteHostname;
		this.AppName = app.Name;
		this.AppResourceGroup = app.ResourceGroupName;
	}

	[Output] public Output<string> Endpoint { get; set; }

	[Output] public Output<string> AppName { get; set; }

	[Output] public Output<string> AppResourceGroup { get; set; }
}
