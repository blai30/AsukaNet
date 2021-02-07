using System.Threading.Tasks;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;

namespace Asuka.Commands
{
    public class CommandModuleBase : ModuleBase<SocketCommandContext>
    {
        // TODO: Add root database controller as protected field.

        protected readonly IOptions<DiscordOptions> Config;
        // protected readonly DbRootController DbRoot;

        protected override void BeforeExecute(CommandInfo command)
        {
            base.BeforeExecute(command);
            Context.Channel.TriggerTypingAsync();
        }

        protected CommandModuleBase(
            IOptions<DiscordOptions> config)
            // DbRootController dbRoot)
        {
            Config = config;
            // DbRoot = dbRoot;
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
        }

        protected Task ReplyReactionAsync(IEmote emote)
        {
            return Context.Message.AddReactionAsync(emote);
        }
    }
}
