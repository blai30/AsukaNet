using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Asuka.Commands.Readers;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace Asuka.Services;

/// <summary>
///     Service to handle command execution from user sent messages.
/// </summary>
public class CommandHandlerService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commandService;
    private readonly IOptions<DiscordOptions> _config;
    private readonly ILogger<CommandHandlerService> _logger;
    private readonly IServiceProvider _provider;

    /// <summary>
    ///     Injected automatically from the service provider.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="commandService"></param>
    /// <param name="config"></param>
    /// <param name="logger"></param>
    /// <param name="provider"></param>
    public CommandHandlerService(
        DiscordSocketClient client,
        CommandService commandService,
        IOptions<DiscordOptions> config,
        ILogger<CommandHandlerService> logger,
        IServiceProvider provider)
    {
        _client = client;
        _commandService = commandService;
        _config = config;
        _logger = logger;
        _provider = provider;
    }

    /// <summary>
    ///     Initialize service at start. Load command type readers and modules.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken stoppingToken)
    {
        // Load custom command type readers. Must be done before loading modules.
        // _commandService.AddTypeReader<IEmote>(new EmoteTypeReader<IEmote>());
        // _commandService.AddTypeReader<IMessage>(new Commands.Readers.MessageTypeReader<IMessage>(), true);
        // _commandService.AddTypeReader<ModuleInfo>(new ModuleInfoTypeReader());
        // _commandService.AddTypeReader<SKColor>(new SKColorTypeReader());

        // Dynamically load all command modules.
        var modules = await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        _logger.LogInformation($"Loaded {modules.Count()} modules");

        _client.MessageReceived += OnMessageReceivedAsync;
        _commandService.CommandExecuted += OnCommandExecutedAsync;

        _logger.LogInformation($"{GetType().Name} started");
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _client.MessageReceived -= OnMessageReceivedAsync;
        _commandService.CommandExecuted -= OnCommandExecutedAsync;

        _logger.LogInformation($"{GetType().Name} stopped");
        await Task.CompletedTask;
    }

    /// <summary>
    ///     Checks message if it is a command then executes it.
    /// </summary>
    /// <param name="socketMessage"></param>
    /// <returns></returns>
    private async Task OnMessageReceivedAsync(SocketMessage socketMessage)
    {
        // Ensure the message is from a user or bot and not a system message.
        if (socketMessage is not SocketUserMessage message) return;

        // Ignore self.
        if (message.Author.Id == _client.CurrentUser.Id) return;

        int argPos = 0;
        // TODO: Fetch custom set guild prefix from database.
        // Check if message has string prefix, only if it is not null.
        string prefix = _config.Value.BotPrefix;
        if (!string.IsNullOrEmpty(prefix) && !message.HasStringPrefix(prefix, ref argPos)) return;

        // Check if message has bot mention prefix and was not invoked by a bot.
        if (message.HasMentionPrefix(_client.CurrentUser, ref argPos) is false &&
            message.Author.IsBot is false)
            return;

        // Create a WebSocket-based command context based on the message.
        var context = new SocketCommandContext(_client, message);
        // Execute the command.
        await _commandService.ExecuteAsync(context, argPos, _provider);
    }

    /// <summary>
    ///     Callback for command execution. Reply with error if command fails execution.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="context"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task OnCommandExecutedAsync(
        Optional<CommandInfo> command,
        ICommandContext context,
        IResult result)
    {
        if (command.IsSpecified is false || result.IsSuccess) return;

        // Command not successful, reply with error.
        await context.Message.ReplyAsync(result.ToString(), allowedMentions: AllowedMentions.None);
    }
}
