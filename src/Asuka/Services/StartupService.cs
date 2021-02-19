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
    ///     Starts the client and logs into discord using bot token.
    /// </summary>
    public class StartupService : IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly IOptions<DiscordOptions> _discord;
        private readonly ILogger<StartupService> _logger;
        private readonly IOptions<TokenOptions> _tokens;

        public StartupService(
            DiscordSocketClient client,
            IOptions<DiscordOptions> discord,
            ILogger<StartupService> logger,
            IOptions<TokenOptions> tokens)
        {
            _client = client;
            _discord = discord;
            _logger = logger;
            _tokens = tokens;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            string token = _tokens.Value.Discord;
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new Exception(
                    "Enter bot token into the `appsettings.json` file found in the application's root directory.");
            }

            // Login to the discord client using bot token.
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await _client.SetActivityAsync(new Game(
                _discord.Value.Status.Game,
                _discord.Value.Status.Activity));

            _logger.LogInformation($"{GetType().Name} started");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{GetType().Name} stopped");
            await Task.CompletedTask;
        }
    }
}
