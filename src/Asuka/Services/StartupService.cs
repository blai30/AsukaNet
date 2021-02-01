using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Asuka.Services
{
    public class StartupService : IHostedService
    {
        private readonly IServiceProvider _provider;
        private readonly IOptions<TokenOptions> _tokens;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public StartupService(
            IServiceProvider provider,
            IOptions<TokenOptions> tokens,
            DiscordSocketClient client,
            CommandService commands)
        {
            _provider = provider;
            _tokens = tokens;
            _client = client;
            _commands = commands;
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
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
