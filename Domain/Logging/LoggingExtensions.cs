using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace TradeProcessor.Domain.Logging;

public static class LoggingExtensions
{
	public static IDisposable BeginScopeWith<T>(this ILogger logger, T state)
	{
		return logger.BeginScope(ToLogStateString(state.ToDictionary()));
	}

	public static IDisposable BeginScopeWith(this ILogger logger, params (string, object)[] properties)
	{
		var dictionary = properties.ToDictionary(p => p.Item1, p => p.Item2)!;

		return logger.BeginScope(ToLogStateString(dictionary));
	}

	// this is a workaround until we can get ConsoleLogging to work properly with state
	private static string ToLogStateString(IDictionary<string, object> stateDictionary) => 
		String.Join(", ", stateDictionary.Select(x => $"{x.Key}:{x.Value}"));

	private static Dictionary<string, object> ToDictionary(this object values)
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
