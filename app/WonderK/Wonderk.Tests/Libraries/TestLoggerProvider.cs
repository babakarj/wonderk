using Microsoft.Extensions.Logging;

namespace Wonderk.Tests.Libraries
{
    /// <summary>
    /// Helper logger provider for capturing log output
    /// </summary>
    public class TestLoggerProvider : ILoggerProvider
    {
        private readonly TestLogger _logger = new();

        public ILogger CreateLogger(string categoryName) => _logger;
        public void Dispose() { }

        public List<string> GetLogMessages() => _logger.Messages;

        private class TestLogger : ILogger
        {
            public List<string> Messages { get; } = [];

            public IDisposable BeginScope<TState>(TState state) => null!;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Messages.Add(formatter(state, exception));
            }
        }
    }
}