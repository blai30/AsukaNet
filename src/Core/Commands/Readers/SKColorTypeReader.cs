using System;
using System.Threading.Tasks;
using Discord.Commands;
using SkiaSharp;

namespace Asuka.Commands.Readers;

public class SKColorTypeReader : TypeReader
{
    public override async Task<TypeReaderResult> ReadAsync(
        ICommandContext context,
        string input,
        IServiceProvider services)
    {
        await Task.Yield();

        // Get color from hex, TryParse will take care of the pound (#) symbol.
        return await Task.FromResult(SKColor.TryParse(input, out var color)
            ? TypeReaderResult.FromSuccess(color)
            : TypeReaderResult.FromError(CommandError.ParseFailed, "Not a valid hex!! (*/ω＼*)"));
    }
}
