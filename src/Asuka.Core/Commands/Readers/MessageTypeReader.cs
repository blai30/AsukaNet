using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Asuka.Commands.Readers
{
    /// <summary>
    /// A <see cref="TypeReader"/> for parsing objects implementing <see cref="IMessage"/>.
    /// This type reader replaces the default <see cref="Discord.Commands.MessageTypeReader{T}"/>
    /// to allow messages to be downloaded instead of pulling only from the cache.
    /// </summary>
    /// <typeparam name="T">The type to be checked; must implement <see cref="IMessage"/>.</typeparam>
    public class MessageTypeReader<T> : TypeReader
        where T : class, IMessage
    {
        /// <inheritdoc />
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            // By Id (1.0)
            if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out ulong id) &&
                await context.Channel.GetMessageAsync(id).ConfigureAwait(false) is T msg)
            {
                return TypeReaderResult.FromSuccess(msg);
            }

            return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Message not found.");
        }
    }
}
