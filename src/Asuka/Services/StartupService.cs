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

            // Login to the discord client with bot token.
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
            _logger.LogTrace(message);

            return Task.CompletedTask;
        }
    }
}
