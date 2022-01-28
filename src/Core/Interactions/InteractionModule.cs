using Asuka.Configuration;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Interactions;

public abstract class InteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    protected InteractionModule(IOptions<DiscordOptions> config, ILogger<InteractionModule> logger)
    {
        Config = config;
        Logger = logger;
    }

    protected IOptions<DiscordOptions> Config { get; }
    protected ILogger<InteractionModule> Logger { get; }
}
