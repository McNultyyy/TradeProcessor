using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Toolbelt.DynamicBinderExtension;

namespace TradeProcessor.Api.Authentication;

public static class AuthenticationBuilderExtensions
{
	public static AuthenticationBuilder AddApiKeyInRequestBody<TApiKeyApiProvider>(
		this AuthenticationBuilder builder)
		where TApiKeyApiProvider : IApiKeyProvider
	{
		var scheme = AuthenticationSchemes.ApiKeyInRequest;

		builder.Services.Configure<ApiKeyOptions>(
			scheme,
			options =>
			{
				options.ToLateBind().Prop["ApiKeyProviderType"] = typeof(TApiKeyApiProvider);
			}
		);

		return builder.AddScheme<ApiKeyOptions, ApiKeyInRequestBodyHandler>(scheme, opts =>
		{
			opts.KeyName = "NO_REQUIRED"; // this field is required by the library. But we are using an interface marker "IApiKeyProperty" instead
			opts.Realm = "Trade Processor";

			opts.Events.OnAuthenticationFailed += context => Task.CompletedTask;
			opts.Events.OnAuthenticationSucceeded += context => Task.CompletedTask;
			opts.Events.OnHandleChallenge += context => Task.CompletedTask;
			opts.Events.OnHandleForbidden += context => Task.CompletedTask;

			// opts.Events.OnValidateKey += context => Task.CompletedTask;

			opts.SuppressWWWAuthenticateHeader = true;
		});
	}
}
