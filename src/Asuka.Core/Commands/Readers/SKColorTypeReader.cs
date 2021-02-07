using System;
using System.Threading.Tasks;
using Discord.Commands;
using SkiaSharp;

namespace Asuka.Commands.Readers
{
    public class SKColorTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            await Task.Yield();

            // Ignore pound sign hex prefix.
            if (input.StartsWith("#"))
            {
                input = input.Substring(1);
            }

            // Get color from hex.
            return SKColor.TryParse(input, out var color) ?
                TypeReaderResult.FromSuccess(color) :
                TypeReaderResult.FromError(CommandError.ParseFailed, "Parameter is not a valid hex.");
        }
    }
}
