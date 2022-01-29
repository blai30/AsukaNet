using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Asuka.Commands.Converters;
using Asuka.Configuration;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace Asuka.Services;

/// <summary>
///     Service to handle command execution from user sent messages.
/// </summary>
public class InteractionHandlerService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IOptions<DiscordOptions> _config;
    private readonly ILogger<InteractionHandlerService> _logger;
    private readonly IServiceProvider _provider;

    /// <summary>
    ///     Injected automatically from the service provider.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="interactionService"></param>
    /// <param name="config"></param>
    /// <param name="logger"></param>
    /// <param name="provider"></param>
    public InteractionHandlerService(
        DiscordSocketClient client,
        InteractionService interactionService,
        IOptions<DiscordOptions> config,
        ILogger<InteractionHandlerService> logger,
        IServiceProvider provider)
    {
        _client = client;
        _interactionService = interactionService;
        _config = config;
        _logger = logger;
        _provider = provider;
    }

    /// <summary>
    ///     Initialize service at start. Load command type readers and modules.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Load custom command type readers. Must be done before loading modules.
        _interactionService.AddTypeConverter<IEmote>(new EmoteTypeConverter<IEmote>());
        _interactionService.AddTypeConverter<IMessage>(new MessageTypeConverter<IMessage>());
        _interactionService.AddTypeConverter<SKColor>(new SKColorTypeConverter());

        // Dynamically load all command modules.
        _client.Ready += OnReadyAsync;
        _client.InteractionCreated += OnInteractionCreatedAsync;
        _interactionService.SlashCommandExecuted += OnInteractionExecutedAsync;

        _logger.LogInformation($"{GetType().Name} started");
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _client.Ready -= OnReadyAsync;
        _client.InteractionCreated -= OnInteractionCreatedAsync;
        _interactionService.SlashCommandExecuted -= OnInteractionExecutedAsync;

        _logger.LogInformation($"{GetType().Name} stopped");
        await Task.CompletedTask;
    }

    private async Task OnReadyAsync()
    {
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
#if DEBUG
        var guildCommands = await _interactionService.RegisterCommandsToGuildAsync(_config.Value.DebugGuildId);
        _logger.LogInformation($"Registered {guildCommands.Count} total application commands in guild {_config.Value.DebugGuildId}");
#endif
        var commands = await _interactionService.RegisterCommandsGloballyAsync();
        _logger.LogInformation($"Registered {commands.Count} total application commands globally");
    }

    private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(_client, interaction);
        await _interactionService.ExecuteCommandAsync(context, _provider);
    }

    /// <summary>
    ///     Callback for command execution. Reply with error if command fails execution.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="context"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task OnInteractionExecutedAsync(
        SlashCommandInfo command,
        IInteractionContext context,
        IResult result)
    {
        if (result.IsSuccess) return;
        // Command not successful, reply with error.
        _logger.LogError(result.ToString());
        _logger.LogError($"{result.ErrorReason}: {result.Error.ToString()}");
        await context.Channel.SendMessageAsync(result.ToString(), allowedMentions: AllowedMentions.None);
    }
}
