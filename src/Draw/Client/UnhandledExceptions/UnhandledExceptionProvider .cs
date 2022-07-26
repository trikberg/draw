using Draw.Client.Services;
using Microsoft.Extensions.Logging;
using System;

namespace Draw.Client.UnhandledExceptions
{
    public class UnhandledExceptionProvider : ILoggerProvider
    {
        public ILoggerService? LoggerService { get; set; }

        public UnhandledExceptionProvider()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new UnhandledExceptionLogger(LoggerService);
        }

        public void Dispose()
        {
        }
    }
}

