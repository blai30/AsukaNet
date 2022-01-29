using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace Asuka.Commands.Converters;

/// <summary>
///     Type reader to parse IEmote to either Emote or Emoji.
/// </summary>
public class EmoteTypeConverter<T> : TypeConverter<T> where T : class, IEmote
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
        // Custom emote.
        if (Emote.TryParse((string) option.Value, out var emote))
        {
            return await Task.FromResult(TypeConverterResult.FromSuccess(emote as T));
        }

        // Unicode emoji.
        var emoji = new Emoji((string) option.Value);
        return await Task.FromResult(string.IsNullOrEmpty(emoji.ToString()) is false
            ? TypeConverterResult.FromSuccess(emoji as T)
            : TypeConverterResult.FromError(
                InteractionCommandError.ParseFailed,
                $"Could not parse emote from input `{option.Value}`."));
    }
}
