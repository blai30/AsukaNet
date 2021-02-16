using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Asuka.Commands.Readers
{
    /// <summary>
    /// Type reader to parse IEmote to either Emote or Emoji.
    /// </summary>
    public class EmoteTypeReader<T> : TypeReader where T : class, IEmote
    {
        public override Task<TypeReaderResult> ReadAsync(
            ICommandContext context,
            string input,
            IServiceProvider services)
        {
            // Custom emote.
            if (Emote.TryParse(input, out var emote))
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(emote as T));
            }

            // Unicode emoji.
            var emoji = new Emoji(input);
            if (!string.IsNullOrEmpty(emoji.ToString()))
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(emoji as T));
            }

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                $"Could not parse emote from input `{input}`."));
        }
    }
}
