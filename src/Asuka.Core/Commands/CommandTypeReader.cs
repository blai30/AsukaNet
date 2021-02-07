using Discord.Commands;

namespace Asuka.Commands
{
    /// <summary>
    /// Abstract class for type reader that includes a protected field for CommandService.
    /// </summary>
    public abstract class CommandTypeReader : TypeReader
    {
        protected readonly CommandService CommandService;

        protected CommandTypeReader(CommandService commandService)
        {
            CommandService = commandService;
        }
    }
}
