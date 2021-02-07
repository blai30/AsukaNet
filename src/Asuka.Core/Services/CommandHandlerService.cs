using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Services
{
    /// <summary>
    /// Service to handle command execution from user sent messages.
    /// </summary>
    public class CommandHandlerService : IHostedService
    {
        private readonly ILogger<CommandHandlerService> _logger;
        private readonly IServiceProvider _provider;
        private readonly IOptions<DiscordOptions> _config;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;

        /// <summary>
        /// Injected automatically from the service provider.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="provider"></param>
        /// <param name="config"></param>
        /// <param name="client"></param>
        /// <param name="commandService"></param>
        public CommandHandlerService(
            ILogger<CommandHandlerService> logger,
            IServiceProvider provider,
            IOptions<DiscordOptions> config,
            DiscordSocketClient client,
            CommandService commandService)
        {
            _logger = logger;
            _provider = provider;
            _config = config;
            _client = client;
            _commandService = commandService;
        }

        /// <summary>
        /// Initialize service at start.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken stoppingToken)
        {
            // Dynamically load all command modules.
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

            _client.MessageReceived += OnMessageReceivedAsync;
            _commandService.CommandExecuted += OnCommandExecutedAsync;

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived -= OnMessageReceivedAsync;
            _commandService.CommandExecuted -= OnCommandExecutedAsync;

            await Task.CompletedTask;
        }

        /// <summary>
        /// Checks message if it is a command then executes it.
        /// </summary>
        /// <param name="socketMessage"></param>
        /// <returns></returns>
        private async Task OnMessageReceivedAsync(SocketMessage socketMessage)
        {
            // Ensure the message is from a user or bot and not a system message.
            if (!(socketMessage is SocketUserMessage message)) return;

            // Ignore self.
            if (message.Author.Id == _client.CurrentUser.Id) return;

            int argPos = 0;
            // TODO: Fetch custom set guild prefix from database.
            // Check if message has string prefix, only if it is not null.
            var prefix = _config.Value.BotPrefix;
            if (prefix != null && !message.HasStringPrefix(prefix, ref argPos))
            {
                return;
            }

            // Check if message has bot mention prefix and was not invoked by a bot.
            if (!message.HasMentionPrefix(_client.CurrentUser, ref argPos) &&
                !message.Author.IsBot)
            {
                return;
            }

            // Create a WebSocket-based command context based on the message.
            var context = new SocketCommandContext(_client, message);
            // Execute the command.
            await _commandService.ExecuteAsync(context, argPos, _provider);
        }

        /// <summary>
        /// Callback for command execution. Reply with error if command fails execution.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified || result.IsSuccess) return;

            // Command not successful, reply with error.
            await context.Message.ReplyAsync(result.ToString());
        }
    }
}
