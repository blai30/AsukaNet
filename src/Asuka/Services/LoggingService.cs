using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace Asuka.Services
{
    public class LoggingService : IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;

        private string LogDirectory { get; set; }
        private string LogFile => Path.Combine(LogDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.log");

        public LoggingService(
            DiscordSocketClient client,
            CommandService commandService)
        {
            _client = client;
            _commandService = commandService;
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            LogDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            _client.Log += OnLogAsync;
            _commandService.Log += OnLogAsync;

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        private Task OnLogAsync(LogMessage log)
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            if (!File.Exists(LogFile))
            {
                File.Create(LogFile).Dispose();
            }

            var logEntry = $"{DateTime.UtcNow:HH:mm:ss} [{log.Severity}] {log.Source}: {log.Exception?.ToString() ?? log.Message}";
            File.AppendAllText(LogFile, $"{logEntry}\n");

            return Console.Out.WriteLineAsync(logEntry);
        }
    }
}
