using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Asuka.Commands
{
    public class CommandModuleBase : ModuleBase<SocketCommandContext>
    {
        // TODO: Add root database controller as protected field.

        public Task ReplyReactionAsync(IEmote emote)
        {
            return Context.Message.AddReactionAsync(emote);
        }
    }
}
