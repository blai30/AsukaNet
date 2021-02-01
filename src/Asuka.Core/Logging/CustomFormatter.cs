using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Asuka.Logging
{
    public class CustomFormatter : ConsoleFormatter, IDisposable
    {
        public CustomFormatter(string name) : base(name)
        {
        }

        public override void Write<TState>(
            in LogEntry<TState> logEntry,
            IExternalScopeProvider scopeProvider,
            TextWriter textWriter)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
