using System.Security.Claims;
using AspNetCore.Authentication.ApiKey;

namespace TradeProcessor.Api.Authentication;

public class ApiKey : IApiKey
{
    public ApiKey(string key)
    {
        Key = key;
        OwnerName = "System";
        Claims = new List<Claim>();
    }

    public string Key { get; }
    public string OwnerName { get; }
    public IReadOnlyCollection<Claim> Claims { get; }
}