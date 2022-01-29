using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using SkiaSharp;

namespace Asuka.Commands.Converters;

public class SKColorTypeConverter : TypeConverter<SKColor>
{
    public override ApplicationCommandOptionType GetDiscordType()
    {
        return ApplicationCommandOptionType.String;
    }

    public override async Task<TypeConverterResult> ReadAsync(
        IInteractionContext context,
        IApplicationCommandInteractionDataOption option,
        IServiceProvider services)
    {
        await Task.Yield();

        // Get color from hex, TryParse will take care of the pound (#) symbol.
        return await Task.FromResult(SKColor.TryParse((string) option.Value, out var color)
            ? TypeConverterResult.FromSuccess(color)
            : TypeConverterResult.FromError(
                InteractionCommandError.ParseFailed,
                "Not a valid hex!! (*/ω＼*)"));
    }
}
