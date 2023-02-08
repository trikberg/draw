using Draw.Client.Services;
using Microsoft.Extensions.Logging;
using System;

namespace Draw.Client.UnhandledExceptions
{
    public class UnhandledExceptionLogger : ILogger
    {
        private ILoggerService? loggerService;

        public UnhandledExceptionLogger(ILoggerService? loggerService)
        {
            this.loggerService = loggerService;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
        {
            if (exception != null && loggerService != null)
            {
                _ = loggerService.Fatal(exception);
            }
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return new NoopDisposable();
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
