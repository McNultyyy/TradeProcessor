using CryptoExchange.Net.Requests;
using System.ComponentModel;

namespace TradeProcessor.Api.Logging;

public static class LoggingExtensions
{
    public static IDisposable? BeginScopeWith(this ILogger logger, object anonymousObject)
    {
        return logger.BeginScope(anonymousObject.ToDictionary());
    }

    private static IEnumerable<KeyValuePair<string, object>> ToDictionary(this object values)
    {
        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (values != null)
        {
            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(values))
            {
                object obj = propertyDescriptor.GetValue(values);
                dict.Add(propertyDescriptor.Name, obj);
            }
        }

        return dict.Select(x => new KeyValuePair<string, object>(x.Key, x.Value));
    }
}