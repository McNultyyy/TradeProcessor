// Copyright 2016-2021, Pulumi Corporation.  All rights reserved.

using Microsoft.AspNetCore.Mvc.Formatters;
using Pulumi;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppService.Inputs;

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
			Sku = new SkuDescriptionArgs
			{
				Tier = "Standard",
				Name = "S1",
			},
		});

		var appInsights = new Component("appInsights", new ComponentArgs
		{
			ResourceName = "TradeProcessor-ai",
			ResourceGroupName = resourceGroup.Name,

			ApplicationType = "web",
			Kind = "web",
			IngestionMode = IngestionMode.ApplicationInsights
		});


		var app = new AppService(
			"TradeProcessor-api",
			new AppServiceArgs()
			{
				Name = "TradeProcessor-api",
				AppServicePlanId = appServicePlan.Id,
				ResourceGroupName = resourceGroup.Name,

				AppSettings = new InputMap<string>()
				{
					{"APPINSIGHTS_INSTRUMENTATIONKEY", appInsights.InstrumentationKey},
					{"APPLICATIONINSIGHTS_CONNECTION_STRING", appInsights.InstrumentationKey.Apply(key => $"InstrumentationKey={key}")},
					{"ApplicationInsightsAgent_EXTENSION_VERSION", "~2"}
				},
				SiteConfig = new AppServiceSiteConfigArgs
				{
					AlwaysOn = true,
					//NetFrameworkVersion = "net6.0",
					HealthCheckPath = "/health",
					MinTlsVersion = "1.2",


				},

				Logs = new AppServiceLogsArgs()
				{
					DetailedErrorMessagesEnabled = true,
					FailedRequestTracingEnabled = true,

					HttpLogs = new AppServiceLogsHttpLogsArgs
					{
						FileSystem = new AppServiceLogsHttpLogsFileSystemArgs
						{
							RetentionInDays = 1,
							RetentionInMb = 35
						},

					},
					ApplicationLogs = new AppServiceLogsApplicationLogsArgs
					{
						FileSystemLevel = "Warning"
					}

				}

			},
			new CustomResourceOptions() { });


		this.Endpoint = app.DefaultSiteHostname;
		this.AppName = app.Name;
		this.AppResourceGroup = app.ResourceGroupName;
	}

	[Output] public Output<string> Endpoint { get; set; }

	[Output] public Output<string> AppName { get; set; }

	[Output] public Output<string> AppResourceGroup { get; set; }
}
