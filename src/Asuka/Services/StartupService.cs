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
        private readonly IServiceProvider _provider;
        private readonly IOptions<TokenOptions> _tokens;
        private readonly DiscordSocketClient _client;

        public StartupService(
            ILogger<StartupService> logger,
            IServiceProvider provider,
            IOptions<TokenOptions> tokens,
            DiscordSocketClient client)
        {
            _logger = logger;
            _provider = provider;
            _tokens = tokens;
            _client = client;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            string token = _tokens.Value.Discord;
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new Exception("Enter bot token into the `appsettings.json` file found in the application's root directory.");
            }

            // Login to the discord client with bot token.
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await _client.SetActivityAsync(new Game(
                "mentions",
                ActivityType.Listening));

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
