using System;
using Asuka.Configuration;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Interactions;

public abstract class InteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private IDisposable _typingState;

    protected InteractionModule(IOptions<DiscordOptions> config, ILogger<InteractionModule> logger)
    {
        Config = config;
        Logger = logger;
    }

    protected IOptions<DiscordOptions> Config { get; }
    protected ILogger<InteractionModule> Logger { get; }

    public override void BeforeExecute(ICommandInfo command)
    {
        _typingState = Context.Channel.EnterTypingState();
    }

    public override void AfterExecute(ICommandInfo command)
    {
        _typingState.Dispose();
    }
}
