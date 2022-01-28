using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;

namespace Asuka.Commands.Converters;

/// <summary>
///     A <see cref="TypeReader" /> for parsing objects implementing <see cref="IMessage" />.
///     This type reader replaces the default <see cref="Discord.Commands.MessageTypeReader{T}" />
///     to allow messages to be downloaded instead of pulling only from the cache.
/// </summary>
/// <typeparam name="T">The type to be checked; must implement <see cref="IMessage" />.</typeparam>
public class MessageTypeConverter<T> : TypeConverter<T>
    where T : class, IMessage
{
    public override ApplicationCommandOptionType GetDiscordType()
    {
        return ApplicationCommandOptionType.String;
    }

    /// <inheritdoc />
    public override async Task<TypeConverterResult> ReadAsync(
        IInteractionContext context,
        IApplicationCommandInteractionDataOption option,
        IServiceProvider services)
    {
        // By Id (1.0)
        if (ulong.TryParse((string) option.Value, NumberStyles.None, CultureInfo.InvariantCulture, out ulong id) &&
            await context.Channel.GetMessageAsync(id).ConfigureAwait(false) is T msg)
        {
            return await Task.FromResult(TypeConverterResult.FromSuccess(msg));
        }

        return await Task.FromResult(TypeConverterResult.FromError(
            InteractionCommandError.BadArgs,
            "Message not found."));
    }
}
