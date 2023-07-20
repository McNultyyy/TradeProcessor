using AspNetCore.Authentication.ApiKey;

namespace TradeProcessor.Api.Authentication;

public class StaticApiKeyProvider : IApiKeyProvider
{
    private readonly IList<string> _allowedKeys;

    public StaticApiKeyProvider(IConfiguration configuration)
    {
        _allowedKeys = configuration.GetRequiredSection("ApiKeys").Get<IList<string>>();
    }

    public async Task<IApiKey> ProvideAsync(string key)
    {
        if (_allowedKeys.Contains(key))
            return new ApiKey(key);

        return null;
    }
}
