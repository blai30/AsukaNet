using System.Collections.Concurrent;
using Asuka.Configuration;
using Microsoft.Extensions.Logging;

namespace Asuka.Logging
{
    [ProviderAlias("FileLogger")]
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly FileLoggerOptions _options;
        private readonly ConcurrentDictionary<string, FileLogger> _loggers;

        public FileLoggerProvider(FileLoggerOptions options)
        {
            _options = options;
            _loggers = new ConcurrentDictionary<string, FileLogger>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _options));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
