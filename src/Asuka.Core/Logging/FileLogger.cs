using System;
using System.IO;
using System.Text;
using Asuka.Configuration;
using Microsoft.Extensions.Logging;

namespace Asuka.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _source;
        private readonly FileLoggerOptions _options;

        private int _duplicateLogFileCount = 0;

        private string LogFile => Path.Combine(AppContext.BaseDirectory, _options.OutputDirectory, GetFileName());

        public FileLogger(string source, FileLoggerOptions options)
        {
            _source = source;
            _options = options;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // Create and format the log entry line.
            var logEntry = $"{DateTime.UtcNow.ToString(_options.DateFormat)} [{logLevel.ToString()}] {_source}: {formatter(state, exception)}\n";

            // Generate directory for log file if it does not exist.
            if (!Directory.Exists(_options.OutputDirectory))
            {
                Directory.CreateDirectory(_options.OutputDirectory);
            }

            // Create a new log file if current file exceeds file size limit.
            var fileInfo = new FileInfo(LogFile);
            if (!fileInfo.Exists)
            {
                fileInfo.Create().Dispose();
            }
            if (fileInfo.Length + logEntry.Length > _options.MaxFileSizeKb * 1000)
            {
                _duplicateLogFileCount++;
            }

            // Write log entry to file.
            File.AppendAllText(LogFile, logEntry);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        /// <summary>
        /// Gets the file name for the logger.
        /// </summary>
        /// <returns>File name</returns>
        private string GetFileName()
        {
            var builder = new StringBuilder(DateTime.UtcNow.ToString(_options.TimeFormat));
            if (_duplicateLogFileCount != 0)
            {
                builder.Append($" ({_duplicateLogFileCount})");
            }
            builder.Append(".log");

            return builder.ToString();
        }
    }
}
