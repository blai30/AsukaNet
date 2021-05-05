using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Asuka.Commands.Readers
{
    /// <summary>
    ///     Type reader to parse IEmote to either Emote or Emoji.
    /// </summary>
    public class EmoteTypeReader<T> : TypeReader where T : class, IEmote
    {
        public override async Task<TypeReaderResult> ReadAsync(
            ICommandContext context,
            string input,
            IServiceProvider services)
        {
            // Custom emote.
            if (Emote.TryParse(input, out var emote))
            {
                return await Task.FromResult(TypeReaderResult.FromSuccess(emote as T));
            }

            // Unicode emoji.
            var emoji = new Emoji(input);
            return await Task.FromResult(string.IsNullOrEmpty(emoji.ToString()) is false
                ? TypeReaderResult.FromSuccess(emoji as T)
                : TypeReaderResult.FromError(CommandError.ParseFailed, $"Could not parse emote from input `{input}`."));
        }
    }
}
