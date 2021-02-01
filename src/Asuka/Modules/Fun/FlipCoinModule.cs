using System;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord.Commands;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Fun
{
    public class FlipCoinModule : CommandModuleBase
    {
        public FlipCoinModule(
            IOptions<DiscordOptions> config)
            : base(config)
        {
        }

        [Command("flipcoin")]
        [Summary("Flips a two-sided coin to determine heads or tails.")]
        public async Task FlipCoinAsync()
        {
            // Generates a random number 0 or 1.
            var seed = Guid.NewGuid().GetHashCode();
            var random = new Random(seed);
            var coin = random.Next(2);

            await ReplyAsync(coin == 0 ? "🪙 **Heads!**" : "🪙 **Tails!**");
        }
    }
}
