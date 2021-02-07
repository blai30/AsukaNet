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
            await Task.CompletedTask;
        }
    }
}
