using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            _client.Ready += OnReadyAsync;
            _client.Log += OnLogAsync;
            _commandService.Log += OnLogAsync;

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _client.Ready -= OnReadyAsync;
            _client.Log -= OnLogAsync;
            _commandService.Log -= OnLogAsync;

            await Task.CompletedTask;
        }

        private Task OnReadyAsync()
        {
            _logger.LogInformation($"Client logged in as {_client.CurrentUser}");
            _logger.LogInformation($"Listening in {_client.Guilds.Count} guilds");

            return Task.CompletedTask;
        }

        private Task OnLogAsync(LogMessage log)
        {
            string message = $"{log.Exception?.ToString() ?? log.Message}";
            _logger.LogInformation(message);

            return Task.CompletedTask;
        }
    }
}
