using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace Asuka.Commands.Readers
{
    public class ModuleTypeReader : CommandTypeReader
    {
        public ModuleTypeReader(CommandService commandService) : base(commandService)
        {
        }

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input,
            IServiceProvider services)
        {
            var moduleName = input.ToUpperInvariant();

            // Get module by name or alias.
            var module = CommandService.Modules.FirstOrDefault(info =>
                info.Name.ToUpperInvariant() == moduleName ||
                info.Aliases.Any(alias => alias.ToUpperInvariant() == moduleName));

            // Module is invalid.
            if (module == null)
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                    $"Module `{input}` does not exist. UwU"));
            }

            return Task.FromResult(TypeReaderResult.FromSuccess(module));
        }
    }
}
