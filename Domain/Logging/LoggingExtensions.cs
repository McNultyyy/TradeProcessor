using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace TradeProcessor.Domain.Logging;

public static class LoggingExtensions
{
	public static IDisposable BeginScopeWith(this ILogger logger, object anonymousObject)
	{
		return logger.BeginScope(anonymousObject.ToDictionary());
	}

	public static IDisposable BeginScopeWith(this ILogger logger, params (string, string)[] properties)
	{
		var dictionary = properties.ToDictionary(p => p.Item1, p => p.Item2)!;
		return logger.BeginScope(dictionary);
	} 

	private static IDictionary<string, object> ToDictionary(this object values)
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

		return dict;
	}
}
