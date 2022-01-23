using System;
using System.Threading.Tasks;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Commands;

public abstract class CommandModuleBase : ModuleBase<SocketCommandContext>
{
    private IDisposable _typing;

    protected CommandModuleBase(IOptions<DiscordOptions> config, ILogger<CommandModuleBase> logger)
    {
        Config = config;
        Logger = logger;
    }

    protected IOptions<DiscordOptions> Config { get; }
    protected ILogger<CommandModuleBase> Logger { get; }

    protected override void BeforeExecute(CommandInfo command)
    {
        base.BeforeExecute(command);
        _typing = Context.Channel.EnterTypingState();
    }

    protected async Task ReplyInlineAsync(
        string message = null,
        bool isTTS = false,
        Embed embed = null,
        RequestOptions options = null,
        AllowedMentions allowedMentions = null,
        MessageReference messageReference = null)
    {
        await Context.Message.ReplyAsync(message, isTTS, embed, allowedMentions, options).ConfigureAwait(false);
        _typing.Dispose();
    }

    protected async Task ReplyReactionAsync(IEmote emote)
    {
        await Context.Message.AddReactionAsync(emote);
    }
}
