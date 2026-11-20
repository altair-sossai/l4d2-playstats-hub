using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace L4D2PlayStats.Hub.Tests.Support;

public class LogCollector : ILoggerProvider
{
    private readonly ConcurrentQueue<(string Category, LogLevel Level, string Message, Exception? Exception)> _entries = new();

    public IReadOnlyList<(string Category, LogLevel Level, string Message, Exception? Exception)> Entries => [.. _entries];

    public ILogger CreateLogger(string categoryName)
    {
        return new CollectingLogger(categoryName, _entries);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private class CollectingLogger(string category, ConcurrentQueue<(string Category, LogLevel Level, string Message, Exception? Exception)> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            entries.Enqueue((category, logLevel, formatter(state, exception), exception));
        }
    }
}