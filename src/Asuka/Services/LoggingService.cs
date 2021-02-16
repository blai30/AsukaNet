using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Asuka.Services
{
    public class LoggingService : IHostedService
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;

        public LoggingService(
            ILogger<LoggingService> logger,
            DiscordSocketClient client,
            CommandService commandService)
        {
            _logger = logger;
            _client = client;
            _commandService = commandService;
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            _client.Ready += OnReadyAsync<StartupService>;
            _client.Log += OnLogAsync<StartupService>;
            _commandService.Log += OnLogAsync<CommandHandlerService>;

            _logger.LogInformation($"{GetType().Name} started");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _client.Ready -= OnReadyAsync<StartupService>;
            _client.Log -= OnLogAsync<StartupService>;
            _commandService.Log -= OnLogAsync<CommandHandlerService>;

            _logger.LogInformation($"{GetType().Name} stopped");
            await Task.CompletedTask;
        }

        private Task OnReadyAsync<T>() where T : IHostedService
        {
            var logger = Log.ForContext<T>();
            logger.Information($"Client logged in as {_client.CurrentUser}");
            logger.Information($"Listening in {_client.Guilds.Count} guilds");

            return Task.CompletedTask;
        }

        private Task OnLogAsync<T>(LogMessage log) where T : IHostedService
        {
            var logger = Log.ForContext<T>();
            string message = $"{log.Exception?.ToString() ?? log.Message}";

            switch (log.Severity)
            {
                case LogSeverity.Critical:
                    logger.Fatal(message);
                    break;
                case LogSeverity.Error:
                    logger.Error(message);
                    break;
                case LogSeverity.Warning:
                    logger.Warning(message);
                    break;
                case LogSeverity.Info:
                    logger.Information(message);
                    break;
                case LogSeverity.Debug:
                    logger.Debug(message);
                    break;
                case LogSeverity.Verbose:
                    logger.Verbose(message);
                    break;
                default:
                    logger.Information(message);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
