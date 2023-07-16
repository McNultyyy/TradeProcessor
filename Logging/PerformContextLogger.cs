using Hangfire.Console;
using Hangfire.Server;

namespace TradeProcessor.Api.Logging;

public class PerformContextLogger<T> : ILogger<T>
{
    private PerformContext _performContext;
    private ILogger<T> _logger;

    public PerformContextLogger(PerformContext performContext, ILogger<T> logger)
    {
        _performContext = performContext;
        _logger = logger;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _logger.Log(logLevel, eventId, state, exception, formatter);

        _performContext.WriteLine(formatter(state, exception));
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _logger.BeginScope(state);
    }
}