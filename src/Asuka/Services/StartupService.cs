using System;
using System.Threading;
using System.Threading.Tasks;
using Asuka.Configuration;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Services
{
    /// <summary>
    /// Starts the client and logs into discord using bot token.
    /// </summary>
    public class StartupService : IHostedService
    {
        private readonly ILogger<StartupService> _logger;
        private readonly IOptions<TokenOptions> _tokens;
        private readonly IOptions<DiscordOptions> _discord;
        private readonly DiscordSocketClient _client;

        public StartupService(
            ILogger<StartupService> logger,
            IOptions<TokenOptions> tokens,
            IOptions<DiscordOptions> discord,
            DiscordSocketClient client)
        {
            _logger = logger;
            _tokens = tokens;
            _discord = discord;
            _client = client;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            string token = _tokens.Value.Discord;
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new Exception("Enter bot token into the `appsettings.json` file found in the application's root directory.");
            }

            _client.Ready += OnReadyAsync;
            _client.Log += OnLogAsync;

            // Login to the discord client using bot token.
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await _client.SetActivityAsync(new Game(
                _discord.Value.Status.Game,
                _discord.Value.Status.Activity));

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _client.Ready -= OnReadyAsync;
            _client.Log -= OnLogAsync;
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

            switch (log.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(message);
                    break;
                case LogSeverity.Error:
                    _logger.LogError(message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(message);
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(message);
                    break;
                case LogSeverity.Verbose:
                    _logger.LogTrace(message);
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(message);
                    break;
                default:
                    _logger.LogInformation(message);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
