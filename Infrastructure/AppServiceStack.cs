// Copyright 2016-2021, Pulumi Corporation.  All rights reserved.

using Pulumi;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using Pulumi.Experimental.Provider;
using ResourceArgs = Pulumi.ResourceArgs;

namespace Infrastructure;

/*
class TelegramBotProvider : Provider
{

}

class TelegramBotResourceArgs : ResourceArgs
{
	[Input("token")]
	public Input<string>? Token { get; set; }


	[Input("setWebhookUrl")]
	public Input<string>? SetWebhookUrl { get; set; }

}
class TelegramBotResource : CustomResource
{
	public TelegramBotResource(TelegramBotResourceArgs args) : base("telegram:bot", "Telegram Bot", args)
	{
		
	}
}
*/

class AppServiceStack : Stack
{
	public AppServiceStack()
	{
		var resourceGroup = new ResourceGroup("TradeProcessor-rg");

		var appServicePlan = new AppServicePlan("TradeProcessor-sp", new AppServicePlanArgs
		{
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
			ApplicationType = "web",
			Kind = "web",
			ResourceGroupName = resourceGroup.Name,
			IngestionMode = IngestionMode.ApplicationInsights
		});

		var app = new WebApp("TradeProcessor-api", new WebAppArgs
		{
			ResourceGroupName = resourceGroup.Name,
			ServerFarmId = appServicePlan.Id,
			SiteConfig = new SiteConfigArgs
			{
				AppSettings = {
					new NameValuePairArgs{
						Name = "APPINSIGHTS_INSTRUMENTATIONKEY",
						Value = appInsights.InstrumentationKey
					},
					new NameValuePairArgs{
						Name = "APPLICATIONINSIGHTS_CONNECTION_STRING",
						Value = appInsights.InstrumentationKey.Apply(key => $"InstrumentationKey={key}"),
					},
					new NameValuePairArgs{
						Name = "ApplicationInsightsAgent_EXTENSION_VERSION",
						Value = "~2",
					}
				},
				AlwaysOn = true,
				//NetFrameworkVersion = "net6.0",
				HealthCheckPath = "/health",
				MinTlsVersion = "1.2",
				DetailedErrorLoggingEnabled = true,
				HttpLoggingEnabled = true,
				RequestTracingEnabled = true
			},
		});

		this.Endpoint = app.DefaultHostName;
		this.AppName = app.Name;
		this.AppResourceGroup = app.ResourceGroup;
	}

	[Output] public Output<string> Endpoint { get; set; }

	[Output] public Output<string> AppName { get; set; }

	[Output] public Output<string> AppResourceGroup { get; set; }
}
