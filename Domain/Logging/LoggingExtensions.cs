using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace TradeProcessor.Domain.Logging;

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
