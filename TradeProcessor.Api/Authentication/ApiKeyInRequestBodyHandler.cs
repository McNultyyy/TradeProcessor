using System.Text.Encodings.Web;
using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TradeProcessor.Api.Authentication;

public class ApiKeyInRequestBodyHandler : ApiKeyHandlerBase
{
    public ApiKeyInRequestBodyHandler(IOptionsMonitor<ApiKeyOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<string> ParseApiKeyAsync()
    {
        try
        {
            Request.EnableBuffering();
            var requestBody = await Request.ReadFromJsonAsync<IDictionary<string, object>>();

            return requestBody[nameof(IApiKeyProperty.ApiKey)].ToString();
        }
        catch
        {
        }   
        finally
        {
            Request.Body.Position = 0;
        }

        return string.Empty;
    }
}