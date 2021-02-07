using Discord.Commands;

namespace Asuka.Commands
{
    public abstract class CommandTypeReader : TypeReader
    {
        protected readonly CommandService CommandService;

        protected CommandTypeReader(CommandService commandService)
        {
            CommandService = commandService;
        }
    }
}
