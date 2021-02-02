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

        public CommandModuleBase(
            IOptions<DiscordOptions> config)
            // DbRootController dbRoot)
        {
            Config = config;
            // DbRoot = dbRoot;
        }

        public Task ReplyReactionAsync(IEmote emote)
        {
            return Context.Message.AddReactionAsync(emote);
        }
    }
}
